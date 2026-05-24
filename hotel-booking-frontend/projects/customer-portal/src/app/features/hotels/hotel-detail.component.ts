import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTableModule } from '@angular/material/table';
import { Router } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { AuthStore } from '@hotel/shared/auth';
import {
  HotelsApiFacade,
  ReviewsApiFacade,
  RoomsApiFacade,
  type HotelDto,
  type ReviewDto,
  type RoomDto,
} from '@hotel/shared/data-access';
import { EmptyStateComponent, LoadingOverlayComponent } from '@hotel/shared/ui';
import {
  ReviewFormDialogComponent,
  type ReviewFormDialogData,
} from '../reviews/review-form-dialog.component';

@Component({
  selector: 'app-hotel-detail',
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatTableModule,
    MatDialogModule,
    CurrencyPipe,
    DatePipe,
    TranslocoPipe,
    EmptyStateComponent,
    LoadingOverlayComponent,
  ],
  template: `
    <hotel-loading-overlay [loading]="loadingHotel()">
      @if (!loadingHotel() && hotel()) {
        <mat-card>
          <mat-card-header>
            <mat-card-title>{{ hotel()!.name }}</mat-card-title>
            <mat-card-subtitle>{{ hotel()!.city }}, {{ hotel()!.country }}</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            @if (hotel()!.address) {
              <p><strong>{{ 'hotels.address' | transloco }}:</strong> {{ hotel()!.address }}</p>
            }
            @if (hotel()!.description) {
              <p>{{ hotel()!.description }}</p>
            }
            @if (hotel()!.rating != null) {
              <p><strong>{{ 'hotels.rating' | transloco }}:</strong> {{ hotel()!.rating }}</p>
            }
          </mat-card-content>
        </mat-card>

        <form class="date-form" [formGroup]="dateForm" (ngSubmit)="searchRooms()">
          <mat-form-field appearance="outline">
            <mat-label>{{ 'hotels.checkIn' | transloco }}</mat-label>
            <input matInput type="date" formControlName="checkIn" />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>{{ 'hotels.checkOut' | transloco }}</mat-label>
            <input matInput type="date" formControlName="checkOut" />
          </mat-form-field>
          <button mat-flat-button color="primary" type="submit" [disabled]="dateForm.invalid || loadingRooms()">
            {{ 'hotels.searchRooms' | transloco }}
          </button>
        </form>

        <hotel-loading-overlay [loading]="loadingRooms()">
          @if (searched() && !loadingRooms()) {
            <h2>{{ 'hotels.availableRooms' | transloco }}</h2>
            @if (rooms().length === 0) {
              <hotel-empty-state titleKey="hotels.noRooms" />
            } @else {
              <table mat-table [dataSource]="rooms()" class="rooms-table">
                <ng-container matColumnDef="roomNumber">
                  <th mat-header-cell *matHeaderCellDef>{{ 'hotels.roomNumber' | transloco }}</th>
                  <td mat-cell *matCellDef="let room">{{ room.roomNumber }}</td>
                </ng-container>
                <ng-container matColumnDef="type">
                  <th mat-header-cell *matHeaderCellDef>{{ 'hotels.type' | transloco }}</th>
                  <td mat-cell *matCellDef="let room">{{ room.type }}</td>
                </ng-container>
                <ng-container matColumnDef="capacity">
                  <th mat-header-cell *matHeaderCellDef>{{ 'hotels.capacity' | transloco }}</th>
                  <td mat-cell *matCellDef="let room">{{ room.capacity }}</td>
                </ng-container>
                <ng-container matColumnDef="price">
                  <th mat-header-cell *matHeaderCellDef>{{ 'hotels.price' | transloco }}</th>
                  <td mat-cell *matCellDef="let room">{{ room.price | currency }}</td>
                </ng-container>
                <ng-container matColumnDef="actions">
                  <th mat-header-cell *matHeaderCellDef></th>
                  <td mat-cell *matCellDef="let room">
                    <button mat-flat-button color="primary" type="button" (click)="book(room)">
                      {{ 'hotels.book' | transloco }}
                    </button>
                  </td>
                </ng-container>
                <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
                <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
              </table>
            }
          }
        </hotel-loading-overlay>

        <section class="reviews-section">
          <div class="reviews-header">
            <h2>{{ 'reviews.title' | transloco }}</h2>
            @if (authStore.isAuthenticated()) {
              <button mat-stroked-button type="button" (click)="openReviewDialog()">
                {{ 'reviews.writeReview' | transloco }}
              </button>
            }
          </div>
          <hotel-loading-overlay [loading]="loadingReviews()">
            @if (!loadingReviews() && reviews().length === 0) {
              <hotel-empty-state titleKey="reviews.noReviews" />
            } @else if (!loadingReviews()) {
              @for (review of reviews(); track review.id) {
                <mat-card class="review-card">
                  <mat-card-content>
                    <p class="review-meta">
                      <strong>{{ review.userName }}</strong>
                      · {{ review.rating }} / 5
                      · {{ review.createdAt | date }}
                    </p>
                    <p>{{ review.comment }}</p>
                  </mat-card-content>
                </mat-card>
              }
            }
          </hotel-loading-overlay>
        </section>
      } @else if (!loadingHotel()) {
        <hotel-empty-state titleKey="hotels.noResults" />
      }
    </hotel-loading-overlay>
  `,
  styles: `
    .date-form {
      display: flex;
      flex-wrap: wrap;
      gap: 1rem;
      align-items: flex-start;
      margin: 1.5rem 0;
    }

    .rooms-table {
      width: 100%;
    }

    .reviews-section {
      margin-top: 2.5rem;
    }

    .reviews-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 1rem;
      margin-bottom: 1rem;
    }

    .review-card {
      margin-bottom: 0.75rem;
    }

    .review-meta {
      margin: 0 0 0.5rem;
      color: var(--hotel-color-text-muted, #64748b);
      font-size: 0.875rem;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HotelDetailComponent {
  readonly id = input.required<string>();

  private readonly fb = inject(FormBuilder);
  private readonly hotelsApi = inject(HotelsApiFacade);
  private readonly roomsApi = inject(RoomsApiFacade);
  private readonly reviewsApi = inject(ReviewsApiFacade);
  private readonly dialog = inject(MatDialog);
  private readonly router = inject(Router);
  readonly authStore = inject(AuthStore);

  readonly loadingHotel = signal(true);
  readonly loadingRooms = signal(false);
  readonly loadingReviews = signal(true);
  readonly searched = signal(false);
  readonly hotel = signal<HotelDto | null>(null);
  readonly rooms = signal<RoomDto[]>([]);
  readonly reviews = signal<ReviewDto[]>([]);

  readonly displayedColumns = ['roomNumber', 'type', 'capacity', 'price', 'actions'];

  readonly dateForm = this.fb.nonNullable.group({
    checkIn: ['', Validators.required],
    checkOut: ['', Validators.required],
  });

  constructor() {
    effect(() => {
      const hotelId = Number(this.id());
      if (!hotelId) {
        return;
      }
      this.loadingHotel.set(true);
      this.hotelsApi.getHotelById(hotelId).subscribe({
        next: (h) => {
          this.hotel.set(h);
          this.loadingHotel.set(false);
          this.loadReviews(hotelId);
        },
        error: () => this.loadingHotel.set(false),
      });
    });
  }

  searchRooms(): void {
    if (this.dateForm.invalid || !this.hotel()?.id) {
      return;
    }
    const { checkIn, checkOut } = this.dateForm.getRawValue();
    this.loadingRooms.set(true);
    this.searched.set(true);
    this.roomsApi
      .getAvailableRooms({
        hotelId: this.hotel()!.id!,
        checkInDate: toIsoDate(checkIn),
        checkOutDate: toIsoDate(checkOut),
      })
      .subscribe({
        next: (list) => {
          this.rooms.set(list);
          this.loadingRooms.set(false);
        },
        error: () => {
          this.rooms.set([]);
          this.loadingRooms.set(false);
        },
      });
  }

  book(room: RoomDto): void {
    const { checkIn, checkOut } = this.dateForm.getRawValue();
    void this.router.navigate(['/booking'], {
      queryParams: {
        hotelId: this.hotel()?.id,
        roomId: room.id,
        checkIn: toIsoDate(checkIn),
        checkOut: toIsoDate(checkOut),
      },
    });
  }

  openReviewDialog(): void {
    const hotelId = this.hotel()?.id;
    if (!hotelId) {
      return;
    }
    const ref = this.dialog.open(ReviewFormDialogComponent, {
      width: '480px',
      data: { hotelId } satisfies ReviewFormDialogData,
    });
    ref.afterClosed().subscribe((saved) => {
      if (saved) {
        this.loadReviews(hotelId);
      }
    });
  }

  private loadReviews(hotelId: number): void {
    this.loadingReviews.set(true);
    this.reviewsApi.getReviewsByHotel(hotelId, 1, 20).subscribe({
      next: (page) => {
        this.reviews.set(page.items ?? []);
        this.loadingReviews.set(false);
      },
      error: () => {
        this.reviews.set([]);
        this.loadingReviews.set(false);
      },
    });
  }
}

function toIsoDate(dateStr: string): string {
  return new Date(dateStr).toISOString();
}
