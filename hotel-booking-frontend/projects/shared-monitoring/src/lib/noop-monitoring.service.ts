import { Injectable } from '@angular/core';
import { MonitoringService } from './monitoring.service';

@Injectable()
export class NoopMonitoringService implements MonitoringService {
  captureException(_error: unknown, _context?: Record<string, unknown>): void {
    // no-op
  }

  captureMessage(_message: string, _context?: Record<string, unknown>): void {
    // no-op
  }
}
