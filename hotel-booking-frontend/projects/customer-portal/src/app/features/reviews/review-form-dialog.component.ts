import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { TranslocoPipe } from '@jsverse/transloco';
import { AuthStore } from '@hotel/shared/auth';
import { ReviewsApiFacade, type ReviewDto } from '@hotel/shared/data-access';
import { ApiBusinessError } from '@hotel/shared/core';

export interface ReviewFormDialogData {
  hotelId: number;
  review?: ReviewDto;
}

@Component({
  selector: 'app-review-form-dialog',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    TranslocoPipe,
  ],
  template: `
    <h2 mat-dialog-title>
      {{ (data.review ? 'reviews.editReview' : 'reviews.writeReview') | transloco }}
    </h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="review-form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>{{ 'reviews.rating' | transloco }}</mat-label>
          <mat-select formControlName="rating">
            @for (star of [5, 4, 3, 2, 1]; track star) {
              <mat-option [value]="star">{{ 'reviews.stars' | transloco: { count: star } }}</mat-option>
            }
          </mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>{{ 'reviews.comment' | transloco }}</mat-label>
          <textarea matInput formControlName="comment" rows="4"></textarea>
        </mat-form-field>
        @if (errorMessage()) {
          <p class="error">{{ errorMessage() }}</p>
        }
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" (click)="dialogRef.close(false)">{{ 'common.cancel' | transloco }}</button>
      <button mat-flat-button color="primary" type="button" [disabled]="form.invalid || saving()" (click)="submit()">
        {{ (data.review ? 'reviews.save' : 'reviews.submit') | transloco }}
      </button>
    </mat-dialog-actions>
  `,
  styles: `
    .review-form {
      display: flex;
      flex-direction: column;
      min-width: 320px;
      padding-top: 0.5rem;
    }
    .full-width {
      width: 100%;
    }
    .error {
      color: #b91c1c;
      margin: 0;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReviewFormDialogComponent {
  readonly data = inject<ReviewFormDialogData>(MAT_DIALOG_DATA);
  readonly dialogRef = inject(MatDialogRef<ReviewFormDialogComponent, boolean>);
  private readonly fb = inject(FormBuilder);
  private readonly reviewsApi = inject(ReviewsApiFacade);
  private readonly authStore = inject(AuthStore);

  readonly saving = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    rating: [this.data.review?.rating ?? 5, [Validators.required, Validators.min(1), Validators.max(5)]],
    comment: [this.data.review?.comment ?? '', [Validators.required, Validators.maxLength(1000)]],
  });

  submit(): void {
    if (this.form.invalid) {
      return;
    }
    const userId = this.authStore.user()?.id;
    if (!userId) {
      return;
    }
    const { rating, comment } = this.form.getRawValue();
    this.saving.set(true);
    this.errorMessage.set(null);

    if (this.data.review?.id) {
      this.reviewsApi.updateReview(this.data.review.id, { rating, comment }).subscribe({
        next: () => this.dialogRef.close(true),
        error: (err: unknown) => {
          this.saving.set(false);
          this.errorMessage.set(err instanceof ApiBusinessError ? err.message : 'Failed');
        },
      });
      return;
    }

    this.reviewsApi
      .createReview(userId, { hotelId: this.data.hotelId, rating, comment })
      .subscribe({
        next: () => this.dialogRef.close(true),
        error: (err: unknown) => {
          this.saving.set(false);
          this.errorMessage.set(err instanceof ApiBusinessError ? err.message : 'Failed');
        },
      });
  }
}
