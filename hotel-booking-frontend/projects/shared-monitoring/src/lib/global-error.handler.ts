import { ErrorHandler, Injectable, inject } from '@angular/core';
import { MONITORING_SERVICE } from './monitoring.service';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  private readonly monitoring = inject(MONITORING_SERVICE);

  handleError(error: unknown): void {
    console.error(error);
    this.monitoring.captureException(error);
  }
}
