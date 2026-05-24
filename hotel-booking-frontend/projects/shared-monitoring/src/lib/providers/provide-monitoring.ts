import { EnvironmentProviders, ErrorHandler, makeEnvironmentProviders } from '@angular/core';
import { GlobalErrorHandler } from '../global-error.handler';
import { MONITORING_SERVICE } from '../monitoring.service';
import { NoopMonitoringService } from '../noop-monitoring.service';

export function provideMonitoring(): EnvironmentProviders {
  return makeEnvironmentProviders([
    { provide: MONITORING_SERVICE, useClass: NoopMonitoringService },
    { provide: ErrorHandler, useClass: GlobalErrorHandler },
  ]);
}
