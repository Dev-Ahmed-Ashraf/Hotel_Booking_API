import { Observable, throwError } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiBusinessError } from '@hotel/shared/core';

export interface ApiEnvelope<T> {
  success?: boolean;
  data?: T;
  message?: string | null;
  errors?: Array<string> | null;
}

export function unwrapApiResponse<T>() {
  return (source: Observable<ApiEnvelope<T> | null | undefined>): Observable<T> =>
    source.pipe(
      map((response) => {
        if (!response?.success) {
          throw new ApiBusinessError(response?.message, response?.errors);
        }
        if (response.data === undefined) {
          throw new ApiBusinessError(response?.message ?? 'No data in response', response?.errors);
        }
        return response.data;
      })
    );
}
