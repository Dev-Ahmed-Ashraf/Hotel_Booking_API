import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { Router, RouterLink } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { AuthStore } from '@hotel/shared/auth';
import { HotelsApiFacade, type HotelDto } from '@hotel/shared/data-access';

@Component({
  selector: 'app-hotel-list',
  imports: [
    ReactiveFormsModule,
    MatTableModule,
    MatButtonModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatInputModule,
    RouterLink,
    TranslocoPipe,
  ],
  template: `
    <div class="page-header">
      <h1>{{ 'admin.hotels.title' | transloco }}</h1>
      @if (isAdmin()) {
        <a mat-flat-button color="primary" routerLink="/hotels/new">
          {{ 'admin.hotels.create' | transloco }}
        </a>
      }
    </div>

    <mat-form-field appearance="outline" class="search-field">
      <mat-label>{{ 'admin.hotels.search' | transloco }}</mat-label>
      <input matInput [formControl]="searchControl" />
    </mat-form-field>

    @if (loading()) {
      <mat-spinner diameter="48" />
    } @else if (hotels().length === 0) {
      <p>{{ 'admin.hotels.noHotels' | transloco }}</p>
    } @else {
      <table mat-table [dataSource]="hotels()" class="data-table">
        <ng-container matColumnDef="name">
          <th mat-header-cell *matHeaderCellDef>{{ 'admin.hotels.name' | transloco }}</th>
          <td mat-cell *matCellDef="let h">{{ h.name }}</td>
        </ng-container>
        <ng-container matColumnDef="city">
          <th mat-header-cell *matHeaderCellDef>{{ 'admin.hotels.city' | transloco }}</th>
          <td mat-cell *matCellDef="let h">{{ h.city }}</td>
        </ng-container>
        <ng-container matColumnDef="country">
          <th mat-header-cell *matHeaderCellDef>{{ 'admin.hotels.country' | transloco }}</th>
          <td mat-cell *matCellDef="let h">{{ h.country }}</td>
        </ng-container>
        <ng-container matColumnDef="rating">
          <th mat-header-cell *matHeaderCellDef>{{ 'admin.hotels.rating' | transloco }}</th>
          <td mat-cell *matCellDef="let h">{{ h.rating }}</td>
        </ng-container>
        <ng-container matColumnDef="actions">
          <th mat-header-cell *matHeaderCellDef>{{ 'admin.hotels.actions' | transloco }}</th>
          <td mat-cell *matCellDef="let h">
            <a mat-button [routerLink]="['/hotels', h.id, 'edit']">{{ 'admin.hotels.edit' | transloco }}</a>
            @if (isAdmin()) {
              <button mat-button color="warn" type="button" (click)="confirmDelete(h)">
                {{ 'admin.hotels.delete' | transloco }}
              </button>
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
    .page-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-bottom: 1rem;
    }
    .search-field {
      width: 100%;
      max-width: 360px;
      display: block;
      margin-bottom: 1rem;
    }
    .data-table {
      width: 100%;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HotelListComponent {
  private readonly hotelsApi = inject(HotelsApiFacade);
  private readonly authStore = inject(AuthStore);

  readonly searchControl = new FormControl('', { nonNullable: true });
  readonly loading = signal(false);
  readonly hotels = signal<HotelDto[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(10);
  readonly isAdmin = signal(false);

  readonly columns = ['name', 'city', 'country', 'rating', 'actions'];

  constructor() {
    this.isAdmin.set(this.authStore.role() === 1);
    this.load();
    this.searchControl.valueChanges.pipe(debounceTime(300), distinctUntilChanged()).subscribe(() => {
      this.pageIndex.set(0);
      this.load();
    });
  }

  onPage(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.load();
  }

  confirmDelete(hotel: HotelDto): void {
    if (!hotel.id || !confirm('Delete this hotel?')) {
      return;
    }
    this.hotelsApi.deleteHotel(hotel.id).subscribe({
      next: () => this.load(),
    });
  }

  private load(): void {
    this.loading.set(true);
    const search = this.searchControl.value.trim();
    this.hotelsApi
      .getHotels({
        pageNumber: this.pageIndex() + 1,
        pageSize: this.pageSize(),
        city: search || undefined,
      })
      .subscribe({
        next: (result) => {
          this.hotels.set(result.items ?? []);
          this.totalCount.set(result.totalCount ?? 0);
          this.loading.set(false);
        },
        error: () => {
          this.hotels.set([]);
          this.loading.set(false);
        },
      });
  }
}
