import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { MatChipsModule } from '@angular/material/chips';
import { TranslocoPipe } from '@jsverse/transloco';
import type { BookingStatus } from '@hotel/shared/data-access';

const STATUS_CLASSES: Record<BookingStatus, string> = {
  0: 'status-pending',
  1: 'status-confirmed',
  2: 'status-cancelled',
  3: 'status-completed',
  4: 'status-noshow',
};

const STATUS_KEYS: Record<BookingStatus, string> = {
  0: 'myBookings.status.pending',
  1: 'myBookings.status.confirmed',
  2: 'myBookings.status.cancelled',
  3: 'myBookings.status.completed',
  4: 'myBookings.status.noShow',
};

@Component({
  selector: 'hotel-status-badge',
  imports: [MatChipsModule, TranslocoPipe],
  template: `
    <mat-chip [class]="statusClass()">{{ statusKey() | transloco }}</mat-chip>
  `,
  styles: `
    .status-pending {
      --mdc-chip-label-text-color: #e65100;
      background: #fff3e0;
    }
    .status-confirmed {
      --mdc-chip-label-text-color: #2e7d32;
      background: #e8f5e9;
    }
    .status-cancelled {
      --mdc-chip-label-text-color: #c62828;
      background: #ffebee;
    }
    .status-completed {
      --mdc-chip-label-text-color: #1565c0;
      background: #e3f2fd;
    }
    .status-noshow {
      --mdc-chip-label-text-color: #6a1b9a;
      background: #f3e5f5;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatusBadgeComponent {
  readonly status = input.required<BookingStatus>();

  readonly statusClass = computed(() => STATUS_CLASSES[this.status()] ?? 'status-pending');
  readonly statusKey = computed(() => STATUS_KEYS[this.status()] ?? STATUS_KEYS[0]);
}
