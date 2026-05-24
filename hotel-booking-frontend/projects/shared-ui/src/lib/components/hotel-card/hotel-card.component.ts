import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { RouterLink } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import type { HotelDto } from '@hotel/shared/data-access';

@Component({
  selector: 'hotel-card',
  imports: [MatCardModule, MatChipsModule, RouterLink, TranslocoPipe],
  template: `
    <mat-card class="hotel-card" [routerLink]="['/hotels', hotel().id]">
      <mat-card-header>
        <mat-card-title>{{ hotel().name }}</mat-card-title>
        <mat-card-subtitle>
          {{ hotel().city }}, {{ hotel().country }}
        </mat-card-subtitle>
      </mat-card-header>
      <mat-card-content>
        @if (hotel().description) {
          <p class="hotel-card__description">{{ hotel().description }}</p>
        }
        @if (hotel().rating != null) {
          <mat-chip-set>
            <mat-chip>{{ 'hotels.rating' | transloco }}: {{ hotel().rating }}</mat-chip>
          </mat-chip-set>
        }
      </mat-card-content>
      <mat-card-actions>
        <a mat-button color="primary" [routerLink]="['/hotels', hotel().id]">
          {{ 'hotels.viewDetails' | transloco }}
        </a>
      </mat-card-actions>
    </mat-card>
  `,
  styles: `
    .hotel-card {
      cursor: pointer;
      height: 100%;
      display: flex;
      flex-direction: column;
    }

    .hotel-card__description {
      display: -webkit-box;
      -webkit-line-clamp: 3;
      -webkit-box-orient: vertical;
      overflow: hidden;
      margin-bottom: 0.5rem;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HotelCardComponent {
  readonly hotel = input.required<HotelDto>();
}
