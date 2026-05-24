import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ThemeService } from '../../services/theme.service';

@Component({
  selector: 'hotel-theme-toggle',
  imports: [MatButtonModule, MatIconModule],
  template: `
    <button mat-icon-button type="button" (click)="theme.toggleTheme()" [attr.aria-label]="ariaLabel()">
      <mat-icon>{{ theme.isDark() ? 'light_mode' : 'dark_mode' }}</mat-icon>
    </button>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ThemeToggleComponent {
  readonly theme = inject(ThemeService);

  ariaLabel(): string {
    return this.theme.isDark() ? 'theme.light' : 'theme.dark';
  }
}
