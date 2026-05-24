import { HttpInterceptorFn } from '@angular/common/http';

/** Phase 0 stub — token refresh will be implemented in a later phase. */
export const refreshInterceptor: HttpInterceptorFn = (req, next) => next(req);
