import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Api } from '../generated/api';
import { apiAdminDashboardGet$Json } from '../generated/functions';
import type { DashboardStatsDto } from '../generated/models/dashboard-stats-dto';
import { unwrapApiResponse } from '../operators/unwrap-api-response.operator';

@Injectable({ providedIn: 'root' })
export class AdminDashboardApiFacade {
  private readonly api = inject(Api);
  private readonly http = inject(HttpClient);

  getDashboardStats(): Observable<DashboardStatsDto> {
    return apiAdminDashboardGet$Json(this.http, this.api.rootUrl).pipe(
      map((response) => response.body),
      unwrapApiResponse<DashboardStatsDto>()
    );
  }
}
