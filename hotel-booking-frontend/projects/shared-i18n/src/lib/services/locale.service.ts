import { DOCUMENT } from '@angular/common';
import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';

export type HotelLang = 'en' | 'ar';

const LANG_STORAGE_KEY = 'hotel-lang';

@Injectable({ providedIn: 'root' })
export class LocaleService {
  private readonly document = inject(DOCUMENT);
  private readonly transloco = inject(TranslocoService);

  private readonly langState = signal<HotelLang>(this.readPersistedLang());

  readonly lang = this.langState.asReadonly();
  readonly dir = computed<'ltr' | 'rtl'>(() => (this.langState() === 'ar' ? 'rtl' : 'ltr'));

  constructor() {
    this.transloco.setActiveLang(this.langState());
    effect(() => {
      const lang = this.langState();
      localStorage.setItem(LANG_STORAGE_KEY, lang);
      this.transloco.setActiveLang(lang);
      const html = this.document.documentElement;
      html.lang = lang;
      html.dir = this.dir();
    });
  }

  setLang(lang: HotelLang): void {
    this.langState.set(lang);
  }

  toggleLang(): void {
    this.setLang(this.langState() === 'en' ? 'ar' : 'en');
  }

  private readPersistedLang(): HotelLang {
    const stored = localStorage.getItem(LANG_STORAGE_KEY);
    return stored === 'ar' ? 'ar' : 'en';
  }
}
