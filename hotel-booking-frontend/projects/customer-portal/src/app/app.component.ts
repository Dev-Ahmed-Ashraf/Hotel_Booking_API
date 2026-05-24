import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { LocaleService } from '@hotel/shared/i18n';
import { ThemeService } from '@hotel/shared/ui';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  template: '<router-outlet />',
})
export class AppComponent {
  constructor() {
    inject(ThemeService);
    inject(LocaleService);
  }
}
