import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ApiBusinessError } from '@hotel/shared/core';
import { MONITORING_SERVICE } from '@hotel/shared/monitoring';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const monitoring = inject(MONITORING_SERVICE);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof ApiBusinessError) {
        monitoring.captureException(error, { url: req.url });
        return throwError(() => error);
      }

      if (error instanceof HttpErrorResponse) {
        const body = error.error as { message?: string; errors?: string[] } | null;
        const apiError = new ApiBusinessError(
          body?.message ?? error.message,
          body?.errors ?? null
        );
        monitoring.captureException(apiError, { status: error.status, url: req.url });
        return throwError(() => apiError);
      }

      monitoring.captureException(error);
      return throwError(() => error);
    })
  );
};
