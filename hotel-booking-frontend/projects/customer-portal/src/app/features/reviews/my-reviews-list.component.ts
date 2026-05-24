import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatTableModule } from '@angular/material/table';
import { TranslocoPipe } from '@jsverse/transloco';
import { AuthStore } from '@hotel/shared/auth';
import { ReviewsApiFacade, type ReviewDto } from '@hotel/shared/data-access';
import { EmptyStateComponent, LoadingOverlayComponent } from '@hotel/shared/ui';
import {
  ReviewFormDialogComponent,
  type ReviewFormDialogData,
} from './review-form-dialog.component';

@Component({
  selector: 'app-my-reviews-list',
  imports: [
    MatTableModule,
    MatButtonModule,
    MatPaginatorModule,
    MatDialogModule,
    DatePipe,
    TranslocoPipe,
    EmptyStateComponent,
    LoadingOverlayComponent,
  ],
  template: `
    <h1>{{ 'reviews.myReviews' | transloco }}</h1>

    <hotel-loading-overlay [loading]="loading()">
      @if (!loading() && reviews().length === 0) {
        <hotel-empty-state
          titleKey="reviews.noReviews"
          messageKey="reviews.leaveFromBooking"
        />
      } @else if (!loading()) {
        <table mat-table [dataSource]="reviews()" class="reviews-table">
          <ng-container matColumnDef="hotelName">
            <th mat-header-cell *matHeaderCellDef>{{ 'hotels.title' | transloco }}</th>
            <td mat-cell *matCellDef="let row">{{ row.hotelName }}</td>
          </ng-container>
          <ng-container matColumnDef="rating">
            <th mat-header-cell *matHeaderCellDef>{{ 'reviews.rating' | transloco }}</th>
            <td mat-cell *matCellDef="let row">{{ row.rating }} / 5</td>
          </ng-container>
          <ng-container matColumnDef="comment">
            <th mat-header-cell *matHeaderCellDef>{{ 'reviews.comment' | transloco }}</th>
            <td mat-cell *matCellDef="let row">{{ row.comment }}</td>
          </ng-container>
          <ng-container matColumnDef="createdAt">
            <th mat-header-cell *matHeaderCellDef>{{ 'reviews.date' | transloco }}</th>
            <td mat-cell *matCellDef="let row">{{ row.createdAt | date }}</td>
          </ng-container>
          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef></th>
            <td mat-cell *matCellDef="let row">
              <button mat-button type="button" (click)="edit(row)">
                {{ 'reviews.editReview' | transloco }}
              </button>
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="columns"></tr>
          <tr mat-row *matRowDef="let row; columns: columns"></tr>
        </table>
        <mat-paginator
          [length]="totalCount()"
          [pageIndex]="pageIndex()"
          [pageSize]="pageSize()"
          [pageSizeOptions]="[5, 10, 20]"
          (page)="onPage($event)"
        />
      }
    </hotel-loading-overlay>
  `,
  styles: `
    .reviews-table {
      width: 100%;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MyReviewsListComponent implements OnInit {
  private readonly reviewsApi = inject(ReviewsApiFacade);
  private readonly authStore = inject(AuthStore);
  private readonly dialog = inject(MatDialog);

  readonly loading = signal(true);
  readonly reviews = signal<ReviewDto[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(10);
  readonly columns = ['hotelName', 'rating', 'comment', 'createdAt', 'actions'];

  ngOnInit(): void {
    this.load();
  }

  onPage(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.load();
  }

  edit(review: ReviewDto): void {
    if (!review.hotelId) {
      return;
    }
    const ref = this.dialog.open(ReviewFormDialogComponent, {
      width: '480px',
      data: { hotelId: review.hotelId, review } satisfies ReviewFormDialogData,
    });
    ref.afterClosed().subscribe((saved) => {
      if (saved) {
        this.load();
      }
    });
  }

  private load(): void {
    const userId = this.authStore.user()?.id;
    if (!userId) {
      this.loading.set(false);
      return;
    }
    this.loading.set(true);
    this.reviewsApi
      .getReviewsByUser(userId, this.pageIndex() + 1, this.pageSize())
      .subscribe({
        next: (page) => {
          this.reviews.set(page.items ?? []);
          this.totalCount.set(page.totalCount ?? 0);
          this.loading.set(false);
        },
        error: () => {
          this.reviews.set([]);
          this.loading.set(false);
        },
      });
  }
}
