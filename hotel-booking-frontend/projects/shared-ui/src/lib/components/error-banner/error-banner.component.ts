import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'hotel-error-banner',
  imports: [MatIconModule],
  template: `
    @if (message()) {
      <div class="error-banner" role="alert">
        <mat-icon>error_outline</mat-icon>
        <span>{{ message() }}</span>
      </div>
    }
  `,
  styles: `
    .error-banner {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.75rem 1rem;
      margin-bottom: 1rem;
      border-radius: 0.5rem;
      background: #fef2f2;
      color: #b91c1c;
      border: 1px solid #fecaca;
    }

    :host-context([data-theme='dark']) .error-banner {
      background: #450a0a;
      color: #fecaca;
      border-color: #7f1d1d;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ErrorBannerComponent {
  readonly message = input<string | null>(null);
}
