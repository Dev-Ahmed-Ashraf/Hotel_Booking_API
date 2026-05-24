import { InjectionToken } from '@angular/core';

export interface MonitoringService {
  captureException(error: unknown, context?: Record<string, unknown>): void;
  captureMessage(message: string, context?: Record<string, unknown>): void;
}

export const MONITORING_SERVICE = new InjectionToken<MonitoringService>('MonitoringService');
