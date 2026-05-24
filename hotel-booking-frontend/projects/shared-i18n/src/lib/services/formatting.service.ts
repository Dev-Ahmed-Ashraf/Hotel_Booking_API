import { Injectable, inject } from '@angular/core';
import { LocaleService } from './locale.service';

@Injectable({ providedIn: 'root' })
export class FormattingService {
  private readonly locale = inject(LocaleService);

  formatDate(value: Date | string | number, options?: Intl.DateTimeFormatOptions): string {
    const date = value instanceof Date ? value : new Date(value);
    return new Intl.DateTimeFormat(this.intlLocale(), options).format(date);
  }

  formatNumber(value: number, options?: Intl.NumberFormatOptions): string {
    return new Intl.NumberFormat(this.intlLocale(), options).format(value);
  }

  formatCurrency(value: number, currency = 'USD'): string {
    return new Intl.NumberFormat(this.intlLocale(), { style: 'currency', currency }).format(value);
  }

  private intlLocale(): string {
    return this.locale.lang() === 'ar' ? 'ar-SA' : 'en-US';
  }
}
