import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHotelAuth, withHotelAuthInterceptors } from '@hotel/shared/auth';
import { provideHotelDataAccess } from '@hotel/shared/data-access';
import { provideHotelI18n } from '@hotel/shared/i18n';
import { provideMonitoring } from '@hotel/shared/monitoring';
import { environment } from '../environments/environment';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes, withComponentInputBinding()),
    provideAnimations(),
    provideHttpClient(withHotelAuthInterceptors()),
    provideHotelDataAccess(environment.apiBaseUrl),
    provideHotelAuth(),
    provideHotelI18n(),
    provideMonitoring(),
  ],
};
