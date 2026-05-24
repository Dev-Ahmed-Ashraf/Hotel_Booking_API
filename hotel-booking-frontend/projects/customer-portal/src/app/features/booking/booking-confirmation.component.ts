import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterLink } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { BookingsApiFacade, type BookingDto } from '@hotel/shared/data-access';
import { StatusBadgeComponent } from '@hotel/shared/ui';

@Component({
  selector: 'app-booking-confirmation',
  imports: [
    MatCardModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    RouterLink,
    TranslocoPipe,
    StatusBadgeComponent,
  ],
  template: `
    @if (loading()) {
      <mat-spinner diameter="48" />
    } @else if (booking()) {
      <mat-card>
        <mat-card-header>
          <mat-card-title>{{ 'booking.confirmationTitle' | transloco }}</mat-card-title>
          <mat-card-subtitle>{{ 'booking.confirmationSubtitle' | transloco }}</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <p>{{ 'booking.bookingId' | transloco }}: {{ booking()!.id }}</p>
          <p>
            {{ 'booking.status' | transloco }}:
            @if (booking()!.status != null) {
              <hotel-status-badge [status]="booking()!.status!" />
            }
          </p>
        </mat-card-content>
        <mat-card-actions>
          <a mat-flat-button color="primary" routerLink="/my-bookings">
            {{ 'booking.viewMyBookings' | transloco }}
          </a>
        </mat-card-actions>
      </mat-card>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BookingConfirmationComponent {
  readonly id = input.required<string>();

  private readonly bookingsApi = inject(BookingsApiFacade);

  readonly loading = signal(true);
  readonly booking = signal<BookingDto | null>(null);

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
}
