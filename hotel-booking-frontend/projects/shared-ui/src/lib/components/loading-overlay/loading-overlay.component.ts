import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'hotel-loading-overlay',
  imports: [MatProgressSpinnerModule],
  template: `
    <div class="overlay-host">
      <ng-content />
      @if (loading()) {
        <div class="overlay" role="status" aria-live="polite">
          <mat-spinner diameter="48" />
        </div>
      }
    </div>
  `,
  styles: `
    .overlay-host {
      position: relative;
      min-height: 4rem;
    }

    .overlay {
      position: absolute;
      inset: 0;
      display: flex;
      align-items: center;
      justify-content: center;
      background: color-mix(in srgb, var(--hotel-color-background) 75%, transparent);
      z-index: 2;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoadingOverlayComponent {
  readonly loading = input(false);
}
