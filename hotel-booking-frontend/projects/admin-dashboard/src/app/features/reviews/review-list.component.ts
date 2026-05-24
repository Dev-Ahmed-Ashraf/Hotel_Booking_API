import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { TranslocoPipe } from '@jsverse/transloco';
import { ReviewsApiFacade, type ReviewDto } from '@hotel/shared/data-access';

@Component({
  selector: 'app-review-list',
  imports: [
    MatTableModule,
    MatButtonModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    DatePipe,
    TranslocoPipe,
  ],
  template: `
    <h1>{{ 'admin.reviews.title' | transloco }}</h1>

    @if (loading()) {
      <mat-spinner diameter="48" />
    } @else if (reviews().length === 0) {
      <p>{{ 'admin.reviews.noReviews' | transloco }}</p>
    } @else {
      <table mat-table [dataSource]="reviews()" class="data-table">
        <ng-container matColumnDef="hotel">
          <th mat-header-cell *matHeaderCellDef>{{ 'admin.reviews.hotel' | transloco }}</th>
          <td mat-cell *matCellDef="let r">{{ r.hotelName }}</td>
        </ng-container>
        <ng-container matColumnDef="user">
          <th mat-header-cell *matHeaderCellDef>{{ 'admin.reviews.user' | transloco }}</th>
          <td mat-cell *matCellDef="let r">{{ r.userName }}</td>
        </ng-container>
        <ng-container matColumnDef="rating">
          <th mat-header-cell *matHeaderCellDef>{{ 'admin.reviews.rating' | transloco }}</th>
          <td mat-cell *matCellDef="let r">{{ r.rating }}</td>
        </ng-container>
        <ng-container matColumnDef="comment">
          <th mat-header-cell *matHeaderCellDef>{{ 'admin.reviews.comment' | transloco }}</th>
          <td mat-cell *matCellDef="let r">{{ r.comment }}</td>
        </ng-container>
        <ng-container matColumnDef="date">
          <th mat-header-cell *matHeaderCellDef>{{ 'admin.reviews.date' | transloco }}</th>
          <td mat-cell *matCellDef="let r">{{ r.createdAt | date }}</td>
        </ng-container>
        <ng-container matColumnDef="actions">
          <th mat-header-cell *matHeaderCellDef>{{ 'admin.reviews.actions' | transloco }}</th>
          <td mat-cell *matCellDef="let r">
            <button mat-button color="warn" type="button" (click)="confirmDelete(r)">
              {{ 'admin.reviews.delete' | transloco }}
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
        (page)="onPage($event)"
      />
    }
  `,
  styles: `
    .data-table {
      width: 100%;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReviewListComponent {
  private readonly reviewsApi = inject(ReviewsApiFacade);

  readonly loading = signal(false);
  readonly reviews = signal<ReviewDto[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(10);

  readonly columns = ['hotel', 'user', 'rating', 'comment', 'date', 'actions'];

  constructor() {
    this.load();
  }

  onPage(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.load();
  }

  confirmDelete(review: ReviewDto): void {
    if (!review.id || !confirm('Remove this review?')) {
      return;
    }
    this.reviewsApi.deleteReview(review.id, true).subscribe({ next: () => this.load() });
  }

  private load(): void {
    this.loading.set(true);
    this.reviewsApi
      .getReviews({
        pageNumber: this.pageIndex() + 1,
        pageSize: this.pageSize(),
      })
      .subscribe({
        next: (result) => {
          this.reviews.set(result.items ?? []);
          this.totalCount.set(result.totalCount ?? 0);
          this.loading.set(false);
        },
        error: () => {
          this.reviews.set([]);
          this.loading.set(false);
        },
      });
  }
}
