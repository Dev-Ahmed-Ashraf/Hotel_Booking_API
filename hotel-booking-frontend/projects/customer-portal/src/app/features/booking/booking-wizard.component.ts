import { CurrencyPipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  inject,
  OnDestroy,
  signal,
  viewChild,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatStepper, MatStepperModule } from '@angular/material/stepper';
import { ActivatedRoute, Router } from '@angular/router';
import { loadStripe, Stripe, StripeElements } from '@stripe/stripe-js';
import { TranslocoPipe } from '@jsverse/transloco';
import { AuthStore } from '@hotel/shared/auth';
import {
  BookingsApiFacade,
  PaymentsApiFacade,
  type BookingPriceResponseDto,
} from '@hotel/shared/data-access';
import { ApiBusinessError } from '@hotel/shared/core';
import { Subscription, switchMap, takeWhile, timer } from 'rxjs';
import { STRIPE_PUBLISHABLE_KEY } from '../../tokens/stripe-publishable-key.token';
import { BookingFlowStore } from '../../stores/booking-flow.store';

const PAYMENT_STATUS_COMPLETED = 1;

@Component({
  selector: 'app-booking-wizard',
  imports: [
    ReactiveFormsModule,
    MatStepperModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    CurrencyPipe,
    TranslocoPipe,
  ],
  template: `
    <h1>{{ 'booking.title' | transloco }}</h1>

    @if (missingParams()) {
      <p>{{ 'booking.missingParams' | transloco }}</p>
    } @else {
      <mat-stepper linear #stepper>
        <mat-step [stepControl]="datesForm" [label]="'booking.stepDates' | transloco">
          <form [formGroup]="datesForm">
            <p>{{ 'booking.hotelId' | transloco }}: {{ hotelId() }}</p>
            <p>{{ 'booking.roomId' | transloco }}: {{ roomId() }}</p>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>{{ 'booking.checkIn' | transloco }}</mat-label>
              <input matInput type="date" formControlName="checkIn" />
            </mat-form-field>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>{{ 'booking.checkOut' | transloco }}</mat-label>
              <input matInput type="date" formControlName="checkOut" />
            </mat-form-field>
            <div class="step-actions">
              <button mat-flat-button color="primary" type="button" (click)="calculatePrice()" [disabled]="datesForm.invalid || loading()">
                {{ 'booking.calculatePrice' | transloco }}
              </button>
              <button mat-stroked-button type="button" (click)="goToReview()" [disabled]="!pricePreview()">
                {{ 'booking.next' | transloco }}
              </button>
            </div>
          </form>
        </mat-step>

        <mat-step [label]="'booking.stepReview' | transloco">
          @if (pricePreview()) {
            <mat-card>
              <mat-card-content>
                <p>{{ 'booking.nights' | transloco }}: {{ pricePreview()!.days }}</p>
                <p>{{ 'booking.roomPrice' | transloco }}: {{ pricePreview()!.roomPrice | currency }}</p>
                <p><strong>{{ 'booking.totalPrice' | transloco }}: {{ pricePreview()!.totalPrice | currency }}</strong></p>
              </mat-card-content>
            </mat-card>
          }
          @if (errorMessage()) {
            <p class="error">{{ errorMessage() }}</p>
          }
          <div class="step-actions">
            <button mat-stroked-button type="button" (click)="stepper.previous()">{{ 'booking.back' | transloco }}</button>
            <button mat-flat-button color="primary" type="button" (click)="createBooking()" [disabled]="loading() || !authStore.isAuthenticated()">
              {{ 'booking.createBooking' | transloco }}
            </button>
          </div>
          @if (!authStore.isAuthenticated()) {
            <p class="hint">{{ 'booking.loginRequired' | transloco }}</p>
          }
        </mat-step>

        <mat-step [label]="'booking.stepPayment' | transloco">
          @if (paymentMessage()) {
            <p [class.error]="paymentFailed()">{{ paymentMessage() }}</p>
          }
          <div #paymentElement class="payment-element"></div>
          <div class="step-actions">
            <button mat-stroked-button type="button" (click)="stepper.previous()" [disabled]="paying()">{{ 'booking.back' | transloco }}</button>
            <button mat-flat-button color="primary" type="button" (click)="pay()" [disabled]="paying() || !clientSecret()">
              {{ 'booking.payNow' | transloco }}
            </button>
          </div>
          @if (paying()) {
            <mat-spinner diameter="32" />
          }
        </mat-step>
      </mat-stepper>
    }
  `,
  styles: `
    .full-width {
      width: 100%;
      display: block;
    }
    .step-actions {
      display: flex;
      gap: 1rem;
      margin-top: 1rem;
    }
    .error {
      color: #c62828;
    }
    .hint {
      color: #666;
      margin-top: 0.5rem;
    }
    .payment-element {
      margin: 1rem 0;
      min-height: 120px;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BookingWizardComponent implements OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly bookingsApi = inject(BookingsApiFacade);
  private readonly paymentsApi = inject(PaymentsApiFacade);
  readonly authStore = inject(AuthStore);
  private readonly flowStore = inject(BookingFlowStore);
  private readonly stripeKey = inject(STRIPE_PUBLISHABLE_KEY);

  readonly stepper = viewChild<MatStepper>('stepper');
  readonly paymentElementRef = viewChild<ElementRef<HTMLDivElement>>('paymentElement');

  readonly loading = signal(false);
  readonly paying = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly pricePreview = signal<BookingPriceResponseDto | null>(null);
  readonly bookingId = signal<number | null>(null);
  readonly clientSecret = signal<string | null>(null);
  readonly paymentMessage = signal<string | null>(null);
  readonly paymentFailed = signal(false);
  readonly hotelId = signal<number | null>(null);
  readonly roomId = signal<number | null>(null);
  readonly missingParams = signal(true);

  readonly datesForm = this.fb.nonNullable.group({
    checkIn: ['', Validators.required],
    checkOut: ['', Validators.required],
  });

  private stripe: Stripe | null = null;
  private elements: StripeElements | null = null;
  private pollSub: Subscription | null = null;

  constructor() {
    this.route.queryParams.subscribe((params) => {
      const hId = Number(params['hotelId']);
      const rId = Number(params['roomId']);
      const checkIn = params['checkIn'] as string | undefined;
      const checkOut = params['checkOut'] as string | undefined;

      if (!hId || !rId || !checkIn || !checkOut) {
        this.missingParams.set(true);
        return;
      }

      this.missingParams.set(false);
      this.hotelId.set(hId);
      this.roomId.set(rId);
      this.datesForm.patchValue({
        checkIn: toDateInputValue(checkIn),
        checkOut: toDateInputValue(checkOut),
      });
      this.flowStore.setParams({ hotelId: hId, roomId: rId, checkIn, checkOut });
    });
  }

  ngOnDestroy(): void {
    this.pollSub?.unsubscribe();
  }

  goToReview(): void {
    this.stepper()?.next();
  }

  calculatePrice(): void {
    if (this.datesForm.invalid || !this.roomId()) {
      return;
    }
    const { checkIn, checkOut } = this.datesForm.getRawValue();
    this.loading.set(true);
    this.bookingsApi
      .calculatePrice({
        roomId: this.roomId()!,
        checkInDate: toIsoDate(checkIn),
        checkOutDate: toIsoDate(checkOut),
      })
      .subscribe({
        next: (preview) => {
          this.pricePreview.set(preview);
          this.flowStore.pricePreview.set(preview);
          this.loading.set(false);
        },
        error: (err: unknown) => {
          this.errorMessage.set(err instanceof ApiBusinessError ? err.message : 'Failed');
          this.loading.set(false);
        },
      });
  }

  createBooking(): void {
    const userId = this.authStore.user()?.id;
    if (!userId || !this.roomId()) {
      return;
    }
    const { checkIn, checkOut } = this.datesForm.getRawValue();
    this.loading.set(true);
    this.errorMessage.set(null);
    this.bookingsApi
      .createBooking(userId, {
        roomId: this.roomId()!,
        checkInDate: toIsoDate(checkIn),
        checkOutDate: toIsoDate(checkOut),
      })
      .subscribe({
        next: async (booking) => {
          this.bookingId.set(booking.id ?? null);
          this.flowStore.booking.set(booking);
          this.loading.set(false);
          this.stepper()?.next();
          await this.initPayment(booking.id!);
        },
        error: (err: unknown) => {
          this.errorMessage.set(err instanceof ApiBusinessError ? err.message : 'Failed');
          this.loading.set(false);
        },
      });
  }

  async pay(): Promise<void> {
    if (!this.stripe || !this.elements || !this.clientSecret()) {
      return;
    }
    this.paying.set(true);
    this.paymentMessage.set(null);
    this.paymentFailed.set(false);

    const { error } = await this.stripe.confirmPayment({
      elements: this.elements,
      confirmParams: { return_url: window.location.href },
      redirect: 'if_required',
    });

    if (error) {
      this.paymentFailed.set(true);
      this.paymentMessage.set(error.message ?? 'Payment failed');
      this.paying.set(false);
      return;
    }

    this.startPolling();
  }

  private async initPayment(bookingId: number): Promise<void> {
    this.paymentsApi.createPaymentIntent(bookingId).subscribe({
      next: async (intent) => {
        const secret = intent.clientSecret ?? null;
        this.clientSecret.set(secret);
        this.flowStore.clientSecret.set(secret);
        if (secret) {
          await this.mountStripe(secret);
        }
      },
      error: (err: unknown) => {
        this.errorMessage.set(err instanceof ApiBusinessError ? err.message : 'Payment init failed');
      },
    });
  }

  private async mountStripe(clientSecret: string): Promise<void> {
    this.stripe = await loadStripe(this.stripeKey);
    if (!this.stripe) {
      return;
    }
    this.elements = this.stripe.elements({ clientSecret });
    const paymentElement = this.elements.create('payment');
    const container = this.paymentElementRef()?.nativeElement;
    if (container) {
      paymentElement.mount(container);
    }
  }

  private startPolling(): void {
    const id = this.bookingId();
    if (!id) {
      return;
    }
    this.paymentMessage.set('Processing payment...');
    this.pollSub?.unsubscribe();
    this.pollSub = timer(0, 2000)
      .pipe(
        switchMap(() => this.paymentsApi.getPaymentByBooking(id)),
        takeWhile((payment) => payment.status !== PAYMENT_STATUS_COMPLETED, true)
      )
      .subscribe({
        next: (payment) => {
          if (payment.status === PAYMENT_STATUS_COMPLETED) {
            this.paying.set(false);
            void this.router.navigate(['/booking/confirmation', id]);
          }
        },
        error: () => {
          this.paymentFailed.set(true);
          this.paymentMessage.set('Payment verification failed');
          this.paying.set(false);
        },
      });
  }
}

function toIsoDate(dateStr: string): string {
  return new Date(dateStr).toISOString();
}

function toDateInputValue(iso: string): string {
  return iso.slice(0, 10);
}
