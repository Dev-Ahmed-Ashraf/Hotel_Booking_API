import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Translation, TranslocoLoader } from '@jsverse/transloco';
import { forkJoin, Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

function loadOptional(http: HttpClient, lang: string, file: string): Observable<Translation> {
  return http.get<Translation>(`/assets/i18n/${lang}/${file}`).pipe(catchError(() => of({})));
}

@Injectable({ providedIn: 'root' })
export class TranslocoHttpLoader implements TranslocoLoader {
  private readonly http = inject(HttpClient);

  getTranslation(lang: string): Observable<Translation> {
    return forkJoin({
      common: this.http.get<Translation>(`/assets/i18n/${lang}/common.json`),
      validation: this.http.get<Translation>(`/assets/i18n/${lang}/validation.json`),
      hotels: loadOptional(this.http, lang, 'hotels.json'),
      booking: loadOptional(this.http, lang, 'booking.json'),
      myBookings: loadOptional(this.http, lang, 'myBookings.json'),
      admin: loadOptional(this.http, lang, 'admin.json'),
      reviews: loadOptional(this.http, lang, 'reviews.json'),
    }).pipe(
      map(({ common, validation, hotels, booking, myBookings, admin, reviews }) => ({
        ...common,
        validation,
        ...hotels,
        ...booking,
        ...myBookings,
        ...admin,
        ...reviews,
      }))
    );
  }
}
