import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { TranslocoPipe } from '@jsverse/transloco';
import { AuthStore } from '@hotel/shared/auth';
import {
  BookingsApiFacade,
  HotelsApiFacade,
  type BookingDto,
  type BookingDtoPagedList,
  type BookingStatus,
  type HotelDto,
} from '@hotel/shared/data-access';
import { StatusBadgeComponent } from '@hotel/shared/ui';

const BOOKING_STATUSES: BookingStatus[] = [0, 1, 2, 3, 4];

@Component({
  selector: 'app-booking-list',
  imports: [
    MatTableModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatSelectModule,
    DatePipe,
    CurrencyPipe,
    TranslocoPipe,
    StatusBadgeComponent,
  ],
  template: `
    <h1>{{ 'admin.bookings.title' | transloco }}</h1>

    <div class="filters">
      @if (isAdmin()) {
        <mat-form-field appearance="outline">
          <mat-label>{{ 'admin.bookings.filterHotel' | transloco }}</mat-label>
          <mat-select [value]="filterHotelId()" (selectionChange)="onHotelFilter($event.value)">
            <mat-option [value]="null">{{ 'admin.bookings.allHotels' | transloco }}</mat-option>
            @for (h of hotels(); track h.id) {
              <mat-option [value]="h.id">{{ h.name }}</mat-option>
            }
          </mat-select>
        </mat-form-field>
      }
      <mat-form-field appearance="outline">
        <mat-label>{{ 'admin.bookings.filterStatus' | transloco }}</mat-label>
        <mat-select [value]="filterStatus()" (selectionChange)="onStatusFilter($event.value)">
          <mat-option [value]="null">{{ 'admin.bookings.allStatuses' | transloco }}</mat-option>
          @for (s of statuses; track s) {
            <mat-option [value]="s">{{ statusLabel(s) }}</mat-option>
          }
        </mat-select>
      </mat-form-field>
    </div>

    @if (loading()) {
      <mat-spinner diameter="48" />
    } @else if (bookings().length === 0) {
      <p>{{ 'admin.bookings.noBookings' | transloco }}</p>
    } @else {
      <table mat-table [dataSource]="bookings()" class="data-table">
        <ng-container matColumnDef="hotel">
          <th mat-header-cell *matHeaderCellDef>{{ 'admin.bookings.hotel' | transloco }}</th>
          <td mat-cell *matCellDef="let b">{{ b.hotelName }}</td>
        </ng-container>
        <ng-container matColumnDef="guest">
          <th mat-header-cell *matHeaderCellDef>{{ 'admin.bookings.guest' | transloco }}</th>
          <td mat-cell *matCellDef="let b">{{ b.userName }}</td>
        </ng-container>
        <ng-container matColumnDef="room">
          <th mat-header-cell *matHeaderCellDef>{{ 'admin.bookings.room' | transloco }}</th>
          <td mat-cell *matCellDef="let b">{{ b.roomNumber }}</td>
        </ng-container>
        <ng-container matColumnDef="checkIn">
          <th mat-header-cell *matHeaderCellDef>{{ 'admin.bookings.checkIn' | transloco }}</th>
          <td mat-cell *matCellDef="let b">{{ b.checkInDate | date }}</td>
        </ng-container>
        <ng-container matColumnDef="checkOut">
          <th mat-header-cell *matHeaderCellDef>{{ 'admin.bookings.checkOut' | transloco }}</th>
          <td mat-cell *matCellDef="let b">{{ b.checkOutDate | date }}</td>
        </ng-container>
        <ng-container matColumnDef="total">
          <th mat-header-cell *matHeaderCellDef>{{ 'admin.bookings.total' | transloco }}</th>
          <td mat-cell *matCellDef="let b">{{ b.totalPrice | currency }}</td>
        </ng-container>
        <ng-container matColumnDef="status">
          <th mat-header-cell *matHeaderCellDef>{{ 'admin.bookings.status' | transloco }}</th>
          <td mat-cell *matCellDef="let b">
            @if (b.status != null) {
              <hotel-status-badge [status]="b.status" />
              <mat-form-field appearance="outline" class="status-select">
                <mat-select [value]="b.status" (selectionChange)="changeStatus(b, $event.value)">
                  @for (s of statuses; track s) {
                    <mat-option [value]="s">{{ statusLabel(s) }}</mat-option>
                  }
                </mat-select>
              </mat-form-field>
            }
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
    .filters {
      display: flex;
      flex-wrap: wrap;
      gap: 1rem;
      margin-bottom: 1rem;
    }
    .data-table {
      width: 100%;
    }
    .status-select {
      width: 160px;
      margin: 0;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BookingListComponent {
  private readonly bookingsApi = inject(BookingsApiFacade);
  private readonly hotelsApi = inject(HotelsApiFacade);
  private readonly authStore = inject(AuthStore);

  readonly statuses = BOOKING_STATUSES;
  readonly loading = signal(false);
  readonly bookings = signal<BookingDto[]>([]);
  readonly hotels = signal<HotelDto[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(10);
  readonly filterHotelId = signal<number | null>(null);
  readonly filterStatus = signal<BookingStatus | null>(null);
  readonly isAdmin = signal(this.authStore.role() === 1);

  readonly columns = ['hotel', 'guest', 'room', 'checkIn', 'checkOut', 'total', 'status'];

  constructor() {
    if (this.isAdmin()) {
      this.hotelsApi.getHotels({ pageSize: 100 }).subscribe({
        next: (r) => this.hotels.set(r.items ?? []),
      });
      this.load();
    } else {
      this.hotelsApi.getHotels({ pageSize: 100 }).subscribe({
        next: (r) => {
          const items = r.items ?? [];
          this.hotels.set(items);
          if (items[0]?.id) {
            this.filterHotelId.set(items[0].id);
          }
          this.load();
        },
      });
    }
  }

  onHotelFilter(hotelId: number | null): void {
    this.filterHotelId.set(hotelId);
    this.pageIndex.set(0);
    this.load();
  }

  onStatusFilter(status: BookingStatus | null): void {
    this.filterStatus.set(status);
    this.pageIndex.set(0);
    this.load();
  }

  onPage(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.load();
  }

  changeStatus(booking: BookingDto, status: BookingStatus): void {
    if (!booking.id || booking.status === status) {
      return;
    }
    this.bookingsApi.changeBookingStatus(booking.id, status).subscribe({
      next: (updated) => {
        this.bookings.update((list) =>
          list.map((b) => (b.id === updated.id ? { ...b, status: updated.status } : b))
        );
      },
    });
  }

  statusLabel(status: BookingStatus): string {
    const labels = ['Pending', 'Confirmed', 'Cancelled', 'Completed', 'No-show'];
    return labels[status] ?? String(status);
  }

  private load(): void {
    this.loading.set(true);
    const page = this.pageIndex() + 1;
    const pageSize = this.pageSize();
    const hotelId = this.filterHotelId();
    const status = this.filterStatus() ?? undefined;

    const handleResult = (result: BookingDtoPagedList) => {
      this.bookings.set(result.items ?? []);
      this.totalCount.set(result.totalCount ?? 0);
      this.loading.set(false);
    };

    if (!this.isAdmin() && hotelId) {
      this.bookingsApi.getBookingsByHotel(hotelId, page, pageSize).subscribe({
        next: handleResult,
        error: () => {
          this.bookings.set([]);
          this.loading.set(false);
        },
      });
      return;
    }

    this.bookingsApi
      .getBookings({
        pageNumber: page,
        pageSize,
        hotelId: hotelId ?? undefined,
        status,
      })
      .subscribe({
        next: handleResult,
        error: () => {
          this.bookings.set([]);
          this.loading.set(false);
        },
      });
  }
}
