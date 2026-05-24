import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';

@Component({
  selector: 'hotel-empty-state',
  imports: [MatButtonModule, MatIconModule, TranslocoPipe],
  template: `
    <div class="empty-state">
      <mat-icon class="empty-state__icon">inbox</mat-icon>
      <h3>{{ titleKey() | transloco }}</h3>
      @if (messageKey()) {
        <p>{{ messageKey()! | transloco }}</p>
      }
      @if (actionKey()) {
        <button mat-flat-button color="primary" type="button" (click)="actionClick.emit()">
          {{ actionKey()! | transloco }}
        </button>
      }
    </div>
  `,
  styles: `
    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 3rem 1.5rem;
      text-align: center;
      color: var(--hotel-color-text-muted, #64748b);
    }

    .empty-state__icon {
      font-size: 3rem;
      width: 3rem;
      height: 3rem;
      margin-bottom: 1rem;
      opacity: 0.6;
    }

    h3 {
      margin: 0 0 0.5rem;
      color: var(--hotel-color-text);
    }

    p {
      margin: 0 0 1.25rem;
      max-width: 28rem;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmptyStateComponent {
  readonly titleKey = input.required<string>();
  readonly messageKey = input<string | undefined>();
  readonly actionKey = input<string | undefined>();
  readonly actionClick = output<void>();
}
