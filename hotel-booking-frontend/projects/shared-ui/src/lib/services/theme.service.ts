import { DOCUMENT } from '@angular/common';
import { Injectable, computed, effect, inject, signal } from '@angular/core';

export type HotelTheme = 'light' | 'dark';

const THEME_STORAGE_KEY = 'hotel-theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);

  private readonly themeState = signal<HotelTheme>(this.readPersistedTheme());

  readonly theme = this.themeState.asReadonly();
  readonly isDark = computed(() => this.themeState() === 'dark');

  constructor() {
    effect(() => {
      const theme = this.themeState();
      localStorage.setItem(THEME_STORAGE_KEY, theme);
      const html = this.document.documentElement;
      html.classList.toggle('dark', theme === 'dark');
      html.setAttribute('data-theme', theme);
    });
  }

  setTheme(theme: HotelTheme): void {
    this.themeState.set(theme);
  }

  toggleTheme(): void {
    this.setTheme(this.themeState() === 'light' ? 'dark' : 'light');
  }

  private readPersistedTheme(): HotelTheme {
    const stored = localStorage.getItem(THEME_STORAGE_KEY);
    return stored === 'dark' ? 'dark' : 'light';
  }
}
