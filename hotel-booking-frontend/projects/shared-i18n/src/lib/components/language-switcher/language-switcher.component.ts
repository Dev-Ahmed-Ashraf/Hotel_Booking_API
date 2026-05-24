import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocaleService, HotelLang } from '../../services/locale.service';

@Component({
  selector: 'hotel-language-switcher',
  imports: [MatButtonToggleModule, TranslocoPipe],
  template: `
    <mat-button-toggle-group
      [value]="locale.lang()"
      (change)="onLangChange($event.value)"
      aria-label="Language"
    >
      <mat-button-toggle value="en">{{ 'lang.en' | transloco }}</mat-button-toggle>
      <mat-button-toggle value="ar">{{ 'lang.ar' | transloco }}</mat-button-toggle>
    </mat-button-toggle-group>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LanguageSwitcherComponent {
  readonly locale = inject(LocaleService);

  onLangChange(lang: HotelLang): void {
    this.locale.setLang(lang);
  }
}
