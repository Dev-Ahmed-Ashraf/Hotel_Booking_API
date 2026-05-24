import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslocoPipe } from '@jsverse/transloco';
import { AdminDashboardApiFacade, type DashboardStatsDto } from '@hotel/shared/data-access';
import { FormattingService } from '@hotel/shared/i18n';

@Component({
  selector: 'app-dashboard',
  imports: [MatCardModule, MatProgressSpinnerModule, TranslocoPipe],
  template: `
    <h1>{{ 'admin.dashboard.title' | transloco }}</h1>

    @if (loading()) {
      <mat-spinner diameter="48" />
    } @else if (error()) {
      <p class="error">{{ 'admin.dashboard.loadError' | transloco }}</p>
    } @else {
      <div class="kpi-grid">
        <mat-card class="kpi-card">
          <mat-card-header>
            <mat-card-title>{{ 'admin.dashboard.users' | transloco }}</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <p class="kpi-value">{{ stats()?.users?.total ?? 0 }}</p>
            <p class="kpi-sub">
              {{ 'admin.dashboard.new30Days' | transloco }}: {{ stats()?.users?.newLast30Days ?? 0 }}
            </p>
            <p class="kpi-sub">
              {{ 'admin.dashboard.growthRate' | transloco }}: {{ formatPercent(stats()?.users?.growthRate) }}
            </p>
          </mat-card-content>
        </mat-card>

        <mat-card class="kpi-card">
          <mat-card-header>
            <mat-card-title>{{ 'admin.dashboard.hotels' | transloco }}</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <p class="kpi-value">{{ stats()?.hotels?.total ?? 0 }}</p>
            <p class="kpi-sub">
              {{ 'admin.dashboard.new30Days' | transloco }}: {{ stats()?.hotels?.newLast30Days ?? 0 }}
            </p>
            <p class="kpi-sub">
              {{ 'admin.dashboard.avgRating' | transloco }}: {{ stats()?.hotels?.averageRating ?? '—' }}
            </p>
          </mat-card-content>
        </mat-card>

        <mat-card class="kpi-card">
          <mat-card-header>
            <mat-card-title>{{ 'admin.dashboard.rooms' | transloco }}</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <p class="kpi-value">{{ stats()?.rooms?.total ?? 0 }}</p>
            <p class="kpi-sub">
              {{ 'admin.dashboard.available' | transloco }}: {{ stats()?.rooms?.available ?? 0 }}
              · {{ 'admin.dashboard.booked' | transloco }}: {{ stats()?.rooms?.booked ?? 0 }}
            </p>
            <p class="kpi-sub">
              {{ 'admin.dashboard.occupancy' | transloco }}: {{ formatPercent(stats()?.rooms?.occupancyRate) }}
            </p>
          </mat-card-content>
        </mat-card>

        <mat-card class="kpi-card">
          <mat-card-header>
            <mat-card-title>{{ 'admin.dashboard.bookings' | transloco }}</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <p class="kpi-value">{{ stats()?.bookings?.total ?? 0 }}</p>
            <p class="kpi-sub">
              {{ 'admin.dashboard.active' | transloco }}: {{ stats()?.bookings?.active ?? 0 }}
              · {{ 'admin.dashboard.cancelled' | transloco }}: {{ stats()?.bookings?.cancelled ?? 0 }}
            </p>
            <p class="kpi-sub">
              {{ 'admin.dashboard.cancellationRate' | transloco }}:
              {{ formatPercent(stats()?.bookings?.cancellationRate) }}
            </p>
          </mat-card-content>
        </mat-card>

        <mat-card class="kpi-card">
          <mat-card-header>
            <mat-card-title>{{ 'admin.dashboard.payments' | transloco }}</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <p class="kpi-value">{{ formatCurrency(stats()?.payments?.totalRevenue) }}</p>
            <p class="kpi-sub">
              {{ 'admin.dashboard.monthlyRevenue' | transloco }}:
              {{ formatCurrency(stats()?.payments?.monthlyRevenue) }}
            </p>
            <p class="kpi-sub">
              {{ 'admin.dashboard.successRate' | transloco }}:
              {{ formatPercent(stats()?.payments?.successRate) }}
            </p>
            <p class="kpi-sub">
              {{ 'admin.dashboard.pendingPayments' | transloco }}: {{ stats()?.payments?.pendingPayments ?? 0 }}
              · {{ 'admin.dashboard.failedPayments' | transloco }}: {{ stats()?.payments?.failedPayments ?? 0 }}
            </p>
          </mat-card-content>
        </mat-card>

        <mat-card class="kpi-card">
          <mat-card-header>
            <mat-card-title>{{ 'admin.dashboard.reviews' | transloco }}</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <p class="kpi-value">{{ stats()?.reviews?.total ?? 0 }}</p>
            <p class="kpi-sub">
              {{ 'admin.dashboard.new30Days' | transloco }}: {{ stats()?.reviews?.newLast30Days ?? 0 }}
            </p>
            <p class="kpi-sub">
              {{ 'admin.dashboard.avgScore' | transloco }}: {{ stats()?.reviews?.averageScore ?? '—' }}
            </p>
          </mat-card-content>
        </mat-card>
      </div>
    }
  `,
  styles: `
    .kpi-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 1rem;
    }
    .kpi-value {
      font-size: 2rem;
      font-weight: 600;
      margin: 0.5rem 0;
    }
    .kpi-sub {
      margin: 0.25rem 0;
      opacity: 0.85;
      font-size: 0.875rem;
    }
    .error {
      color: #c62828;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent {
  private readonly dashboardApi = inject(AdminDashboardApiFacade);
  private readonly formatting = inject(FormattingService);

  readonly loading = signal(true);
  readonly error = signal(false);
  readonly stats = signal<DashboardStatsDto | null>(null);

  constructor() {
    this.dashboardApi.getDashboardStats().subscribe({
      next: (data) => {
        this.stats.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set(true);
        this.loading.set(false);
      },
    });
  }

  formatCurrency(value: number | undefined | null): string {
    if (value == null) {
      return '—';
    }
    return this.formatting.formatCurrency(value);
  }

  formatPercent(value: number | undefined | null): string {
    if (value == null) {
      return '—';
    }
    return this.formatting.formatNumber(value, { style: 'percent', maximumFractionDigits: 1 });
  }
}
