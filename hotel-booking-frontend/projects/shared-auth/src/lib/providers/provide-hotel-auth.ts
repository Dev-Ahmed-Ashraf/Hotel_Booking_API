import { EnvironmentProviders, makeEnvironmentProviders } from '@angular/core';
import { HttpInterceptorFn, withInterceptors } from '@angular/common/http';
import { authInterceptor } from '../interceptors/auth.interceptor';
import { errorInterceptor } from '../interceptors/error.interceptor';
import { loadingInterceptor } from '../interceptors/loading.interceptor';
import { refreshInterceptor } from '../interceptors/refresh.interceptor';

export const hotelAuthInterceptors: HttpInterceptorFn[] = [
  loadingInterceptor,
  authInterceptor,
  refreshInterceptor,
  errorInterceptor,
];

export function withHotelAuthInterceptors() {
  return withInterceptors(hotelAuthInterceptors);
}

export function provideHotelAuth(): EnvironmentProviders {
  return makeEnvironmentProviders([]);
}
