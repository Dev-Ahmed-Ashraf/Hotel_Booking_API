import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Api } from '../generated/api';
import {
  apiAuthLoginPost$Json,
  apiAuthRegisterPost$Json,
} from '../generated/functions';
import type { AuthResponseDto } from '../generated/models/auth-response-dto';
import type { CreateUserDto } from '../generated/models/create-user-dto';
import type { LoginDto } from '../generated/models/login-dto';
import { unwrapApiResponse } from '../operators/unwrap-api-response.operator';

@Injectable({ providedIn: 'root' })
export class AuthApiFacade {
  private readonly api = inject(Api);
  private readonly http = inject(HttpClient);

  login(body: LoginDto): Observable<AuthResponseDto> {
    return apiAuthLoginPost$Json(this.http, this.api.rootUrl, { body }).pipe(
      map((response) => response.body),
      unwrapApiResponse<AuthResponseDto>()
    );
  }

  register(body: CreateUserDto): Observable<AuthResponseDto> {
    return apiAuthRegisterPost$Json(this.http, this.api.rootUrl, { body }).pipe(
      map((response) => response.body),
      unwrapApiResponse<AuthResponseDto>()
    );
  }
}
