import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { AuthStore } from '@hotel/shared/auth';
import { BookingsApiFacade, type BookingDto } from '@hotel/shared/data-access';
import { StatusBadgeComponent } from '@hotel/shared/ui';

@Component({
  selector: 'app-my-bookings-list',
  imports: [
    MatTableModule,
    MatButtonModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    RouterLink,
    DatePipe,
    CurrencyPipe,
    TranslocoPipe,
    StatusBadgeComponent,
  ],
  template: `
    <h1>{{ 'myBookings.title' | transloco }}</h1>

    @if (loading()) {
      <mat-spinner diameter="48" />
    } @else if (bookings().length === 0) {
      <p>{{ 'myBookings.noBookings' | transloco }}</p>
    } @else {
      <table mat-table [dataSource]="bookings()" class="bookings-table">
        <ng-container matColumnDef="hotel">
          <th mat-header-cell *matHeaderCellDef>{{ 'myBookings.hotel' | transloco }}</th>
          <td mat-cell *matCellDef="let b">{{ b.hotelName }}</td>
        </ng-container>
        <ng-container matColumnDef="room">
          <th mat-header-cell *matHeaderCellDef>{{ 'myBookings.room' | transloco }}</th>
          <td mat-cell *matCellDef="let b">{{ b.roomNumber }}</td>
        </ng-container>
        <ng-container matColumnDef="checkIn">
          <th mat-header-cell *matHeaderCellDef>{{ 'myBookings.checkIn' | transloco }}</th>
          <td mat-cell *matCellDef="let b">{{ b.checkInDate | date }}</td>
        </ng-container>
        <ng-container matColumnDef="checkOut">
          <th mat-header-cell *matHeaderCellDef>{{ 'myBookings.checkOut' | transloco }}</th>
          <td mat-cell *matCellDef="let b">{{ b.checkOutDate | date }}</td>
        </ng-container>
        <ng-container matColumnDef="total">
          <th mat-header-cell *matHeaderCellDef>{{ 'myBookings.total' | transloco }}</th>
          <td mat-cell *matCellDef="let b">{{ b.totalPrice | currency }}</td>
        </ng-container>
        <ng-container matColumnDef="status">
          <th mat-header-cell *matHeaderCellDef>{{ 'myBookings.statusLabel' | transloco }}</th>
          <td mat-cell *matCellDef="let b">
            @if (b.status != null) {
              <hotel-status-badge [status]="b.status" />
            }
          </td>
        </ng-container>
        <ng-container matColumnDef="actions">
          <th mat-header-cell *matHeaderCellDef>{{ 'myBookings.actions' | transloco }}</th>
          <td mat-cell *matCellDef="let b">
            <a mat-button [routerLink]="['/my-bookings', b.id]">{{ 'myBookings.view' | transloco }}</a>
          </td>
        </ng-container>
        <tr mat-header-row *matHeaderRowDef="columns"></tr>
        <tr mat-row *matRowDef="let row; columns: columns"></tr>
      </table>
      <mat-paginator
        [length]="totalCount()"
        [pageIndex]="pageIndex()"
        [pageSize]="pageSize()"
        (page)="onPage($event)"
      />
    }
  `,
  styles: `
    .bookings-table {
      width: 100%;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MyBookingsListComponent {
  private readonly authStore = inject(AuthStore);
  private readonly bookingsApi = inject(BookingsApiFacade);

  readonly loading = signal(false);
  readonly bookings = signal<BookingDto[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(10);

  readonly columns = ['hotel', 'room', 'checkIn', 'checkOut', 'total', 'status', 'actions'];

  constructor() {
    this.load();
  }

  onPage(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.load();
  }

  private load(): void {
    const userId = this.authStore.user()?.id;
    if (!userId) {
      return;
    }
    this.loading.set(true);
    this.bookingsApi.getUserBookings(userId, this.pageIndex() + 1, this.pageSize()).subscribe({
      next: (result) => {
        this.bookings.set(result.items ?? []);
        this.totalCount.set(result.totalCount ?? 0);
        this.loading.set(false);
      },
      error: () => {
        this.bookings.set([]);
        this.loading.set(false);
      },
    });
  }
}
