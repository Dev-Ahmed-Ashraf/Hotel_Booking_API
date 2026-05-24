import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { BookingsApiFacade, type BookingDto } from '@hotel/shared/data-access';
import { ApiBusinessError } from '@hotel/shared/core';
import { StatusBadgeComponent } from '@hotel/shared/ui';

@Component({
  selector: 'app-cancel-booking-dialog',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    TranslocoPipe,
  ],
  template: `
    <h2 mat-dialog-title>{{ 'myBookings.cancelTitle' | transloco }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>{{ 'myBookings.cancelReason' | transloco }}</mat-label>
          <textarea matInput formControlName="reason" rows="3"></textarea>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>{{ 'myBookings.close' | transloco }}</button>
      <button mat-flat-button color="warn" [disabled]="form.invalid" [mat-dialog-close]="form.value.reason">
        {{ 'myBookings.confirmCancel' | transloco }}
      </button>
    </mat-dialog-actions>
  `,
  styles: `.full-width { width: 100%; }`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CancelBookingDialogComponent {
  private readonly fb = inject(FormBuilder);
  readonly form = this.fb.nonNullable.group({
    reason: ['', Validators.required],
  });
}

@Component({
  selector: 'app-my-booking-detail',
  imports: [
    MatCardModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    DatePipe,
    CurrencyPipe,
    TranslocoPipe,
    StatusBadgeComponent,
  ],
  template: `
    @if (loading()) {
      <mat-spinner diameter="48" />
    } @else if (booking()) {
      <mat-card>
        <mat-card-header>
          <mat-card-title>{{ 'myBookings.details' | transloco }}</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <p>{{ 'myBookings.bookingId' | transloco }}: {{ booking()!.id }}</p>
          <p>{{ 'myBookings.hotel' | transloco }}: {{ booking()!.hotelName }}</p>
          <p>{{ 'myBookings.room' | transloco }}: {{ booking()!.roomNumber }}</p>
          <p>{{ 'myBookings.checkIn' | transloco }}: {{ booking()!.checkInDate | date }}</p>
          <p>{{ 'myBookings.checkOut' | transloco }}: {{ booking()!.checkOutDate | date }}</p>
          <p>{{ 'myBookings.total' | transloco }}: {{ booking()!.totalPrice | currency }}</p>
          <p>
            {{ 'myBookings.statusLabel' | transloco }}:
            @if (booking()!.status != null) {
              <hotel-status-badge [status]="booking()!.status!" />
            }
          </p>
          @if (booking()!.cancellationReason) {
            <p>{{ 'myBookings.cancellationReason' | transloco }}: {{ booking()!.cancellationReason }}</p>
          }
        </mat-card-content>
        @if (canLeaveReview()) {
          <p class="review-hint">{{ 'reviews.leaveFromBooking' | transloco }}</p>
          <button mat-stroked-button type="button" (click)="goToHotels()">
            {{ 'nav.hotels' | transloco }}
          </button>
        }
        @if (canCancel()) {
          <mat-card-actions>
            <button mat-flat-button color="warn" type="button" (click)="openCancelDialog()">
              {{ 'myBookings.cancel' | transloco }}
            </button>
          </mat-card-actions>
        }
      </mat-card>
      @if (errorMessage()) {
        <p class="error">{{ errorMessage() }}</p>
      }
    }
  `,
  styles: `
    .error { color: #c62828; margin-top: 1rem; }
    .review-hint { margin: 1rem 0 0.5rem; color: var(--hotel-color-text-muted, #64748b); }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MyBookingDetailComponent {
  readonly id = input.required<string>();

  private readonly bookingsApi = inject(BookingsApiFacade);
  private readonly dialog = inject(MatDialog);
  private readonly router = inject(Router);

  readonly loading = signal(true);
  readonly booking = signal<BookingDto | null>(null);
  readonly errorMessage = signal<string | null>(null);

  constructor() {
    effect(() => {
      const bookingId = Number(this.id());
      if (!bookingId) {
        return;
      }
      this.bookingsApi.getBookingById(bookingId).subscribe({
        next: (b) => {
          this.booking.set(b);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
    });
  }

  canCancel(): boolean {
    const status = this.booking()?.status;
    return status === 0 || status === 1;
  }

  canLeaveReview(): boolean {
    return this.booking()?.status === 3;
  }

  goToHotels(): void {
    void this.router.navigate(['/hotels']);
  }

  openCancelDialog(): void {
    const ref = this.dialog.open(CancelBookingDialogComponent, { width: '400px' });
    ref.afterClosed().subscribe((reason: string | undefined) => {
      if (!reason || !this.booking()?.id) {
        return;
      }
      this.bookingsApi.cancelBooking(this.booking()!.id!, reason).subscribe({
        next: () => void this.router.navigate(['/my-bookings']),
        error: (err: unknown) => {
          this.errorMessage.set(err instanceof ApiBusinessError ? err.message : 'Cancel failed');
        },
      });
    });
  }
}
