import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { HotelsApiFacade } from '@hotel/shared/data-access';
import { ApiBusinessError } from '@hotel/shared/core';

@Component({
  selector: 'app-hotel-form',
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    RouterLink,
    TranslocoPipe,
  ],
  template: `
    <mat-card>
      <mat-card-header>
        <mat-card-title>
          {{ (isEdit() ? 'admin.hotels.editTitle' : 'admin.hotels.createTitle') | transloco }}
        </mat-card-title>
      </mat-card-header>
      <mat-card-content>
        @if (loading()) {
          <mat-spinner diameter="48" />
        } @else {
          <form [formGroup]="form" (ngSubmit)="submit()">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>{{ 'admin.hotels.name' | transloco }}</mat-label>
              <input matInput formControlName="name" />
            </mat-form-field>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>{{ 'admin.hotels.city' | transloco }}</mat-label>
              <input matInput formControlName="city" />
            </mat-form-field>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>{{ 'admin.hotels.country' | transloco }}</mat-label>
              <input matInput formControlName="country" />
            </mat-form-field>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>{{ 'admin.hotels.address' | transloco }}</mat-label>
              <input matInput formControlName="address" />
            </mat-form-field>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>{{ 'admin.hotels.rating' | transloco }}</mat-label>
              <input matInput type="number" formControlName="rating" min="0" max="5" step="0.1" />
            </mat-form-field>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>{{ 'admin.hotels.description' | transloco }}</mat-label>
              <textarea matInput formControlName="description" rows="4"></textarea>
            </mat-form-field>
            @if (errorMessage()) {
              <p class="error">{{ errorMessage() }}</p>
            }
            <div class="actions">
              <button mat-flat-button color="primary" type="submit" [disabled]="form.invalid || saving()">
                {{ 'admin.hotels.save' | transloco }}
              </button>
              <a mat-button routerLink="/hotels">{{ 'admin.hotels.cancel' | transloco }}</a>
            </div>
          </form>
        }
      </mat-card-content>
    </mat-card>
  `,
  styles: `
    .full-width {
      width: 100%;
      display: block;
    }
    .actions {
      display: flex;
      gap: 0.5rem;
      margin-top: 1rem;
    }
    .error {
      color: #c62828;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HotelFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly hotelsApi = inject(HotelsApiFacade);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly isEdit = signal(false);
  readonly errorMessage = signal<string | null>(null);

  private hotelId: number | null = null;

  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    city: [''],
    country: [''],
    address: [''],
    description: [''],
    rating: [0, [Validators.min(0), Validators.max(5)]],
  });

  constructor() {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.hotelId = Number(idParam);
      this.isEdit.set(true);
      this.loadHotel(this.hotelId);
    }
  }

  submit(): void {
    if (this.form.invalid) {
      return;
    }
    this.saving.set(true);
    this.errorMessage.set(null);
    const body = this.form.getRawValue();

    const request$ = this.isEdit() && this.hotelId
      ? this.hotelsApi.updateHotel(this.hotelId, body)
      : this.hotelsApi.createHotel(body);

    request$.subscribe({
      next: () => void this.router.navigateByUrl('/hotels'),
      error: (err: unknown) => {
        this.saving.set(false);
        this.errorMessage.set(err instanceof ApiBusinessError ? err.message : 'admin.common.error');
      },
      complete: () => this.saving.set(false),
    });
  }

  private loadHotel(id: number): void {
    this.loading.set(true);
    this.hotelsApi.getHotelById(id).subscribe({
      next: (hotel) => {
        this.form.patchValue({
          name: hotel.name ?? '',
          city: hotel.city ?? '',
          country: hotel.country ?? '',
          address: hotel.address ?? '',
          description: hotel.description ?? '',
          rating: hotel.rating ?? 0,
        });
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        void this.router.navigateByUrl('/hotels');
      },
    });
  }
}
