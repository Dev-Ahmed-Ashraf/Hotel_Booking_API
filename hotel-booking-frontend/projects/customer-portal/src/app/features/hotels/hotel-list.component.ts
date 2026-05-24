import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { TranslocoPipe } from '@jsverse/transloco';
import { HotelsApiFacade, type HotelDto } from '@hotel/shared/data-access';
import { EmptyStateComponent, HotelCardComponent, LoadingOverlayComponent } from '@hotel/shared/ui';

@Component({
  selector: 'app-hotel-list',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatPaginatorModule,
    TranslocoPipe,
    HotelCardComponent,
    EmptyStateComponent,
    LoadingOverlayComponent,
  ],
  template: `
    <h1>{{ 'hotels.title' | transloco }}</h1>

    <form class="filters" [formGroup]="filterForm" (ngSubmit)="search()">
      <mat-form-field appearance="outline">
        <mat-label>{{ 'hotels.city' | transloco }}</mat-label>
        <input matInput formControlName="city" />
      </mat-form-field>
      <mat-form-field appearance="outline">
        <mat-label>{{ 'hotels.country' | transloco }}</mat-label>
        <input matInput formControlName="country" />
      </mat-form-field>
      <button mat-flat-button color="primary" type="submit">{{ 'hotels.search' | transloco }}</button>
      <button mat-stroked-button type="button" (click)="clear()">{{ 'hotels.clear' | transloco }}</button>
    </form>

    <hotel-loading-overlay [loading]="loading()">
      @if (!loading() && hotels().length === 0) {
        <hotel-empty-state titleKey="hotels.noResults" />
      } @else if (!loading()) {
        <div class="hotel-grid">
          @for (hotel of hotels(); track hotel.id) {
            <hotel-card [hotel]="hotel" />
          }
        </div>
        <mat-paginator
          [length]="totalCount()"
          [pageIndex]="pageIndex()"
          [pageSize]="pageSize()"
          [pageSizeOptions]="[6, 12, 24]"
          (page)="onPage($event)"
        />
      }
    </hotel-loading-overlay>
  `,
  styles: `
    .filters {
      display: flex;
      flex-wrap: wrap;
      gap: 1rem;
      align-items: flex-start;
      margin-bottom: 1.5rem;
    }

    .hotel-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 1.25rem;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HotelListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly hotelsApi = inject(HotelsApiFacade);

  readonly loading = signal(true);
  readonly hotels = signal<HotelDto[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(12);

  readonly filterForm = this.fb.nonNullable.group({
    city: [''],
    country: [''],
  });

  ngOnInit(): void {
    this.load();
  }

  search(): void {
    this.pageIndex.set(0);
    this.load();
  }

  clear(): void {
    this.filterForm.reset();
    this.pageIndex.set(0);
    this.load();
  }

  onPage(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.load();
  }

  private load(): void {
    const { city, country } = this.filterForm.getRawValue();
    this.loading.set(true);
    this.hotelsApi
      .getHotels({
        pageNumber: this.pageIndex() + 1,
        pageSize: this.pageSize(),
        city: city || undefined,
        country: country || undefined,
      })
      .subscribe({
        next: (page) => {
          this.hotels.set(page.items ?? []);
          this.totalCount.set(page.totalCount ?? 0);
          this.loading.set(false);
        },
        error: () => {
          this.hotels.set([]);
          this.loading.set(false);
        },
      });
  }
}
