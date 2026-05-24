import { EnvironmentProviders, makeEnvironmentProviders } from '@angular/core';
import { provideApiConfiguration } from '../generated/api-configuration';

export function resolveApiRootUrl(apiBaseUrl: string): string {
  return apiBaseUrl.replace(/\/api\/?$/, '');
}

export function provideHotelDataAccess(apiBaseUrl: string): EnvironmentProviders {
  return makeEnvironmentProviders([provideApiConfiguration(resolveApiRootUrl(apiBaseUrl))]);
}
