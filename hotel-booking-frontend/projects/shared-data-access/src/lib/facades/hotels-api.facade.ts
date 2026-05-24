import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Api } from '../generated/api';
import {
  apiHotelsGet$Json,
  apiHotelsIdDelete$Json,
  apiHotelsIdGet$Json,
  apiHotelsIdPatch$Json,
  apiHotelsPost$Json,
} from '../generated/functions';
import type { ApiHotelsGet$Json$Params } from '../generated/fn/hotels/api-hotels-get-json';
import type { CreateHotelDto } from '../generated/models/create-hotel-dto';
import type { HotelDto } from '../generated/models/hotel-dto';
import type { HotelDtoPagedList } from '../generated/models/hotel-dto-paged-list';
import type { UpdateHotelDto } from '../generated/models/update-hotel-dto';
import { unwrapApiResponse } from '../operators/unwrap-api-response.operator';

@Injectable({ providedIn: 'root' })
export class HotelsApiFacade {
  private readonly api = inject(Api);
  private readonly http = inject(HttpClient);

  getHotels(params?: ApiHotelsGet$Json$Params): Observable<HotelDtoPagedList> {
    return apiHotelsGet$Json(this.http, this.api.rootUrl, params).pipe(
      map((response) => response.body),
      unwrapApiResponse<HotelDtoPagedList>()
    );
  }

  getHotelById(id: number): Observable<HotelDto> {
    return apiHotelsIdGet$Json(this.http, this.api.rootUrl, { id }).pipe(
      map((response) => response.body),
      unwrapApiResponse<HotelDto>()
    );
  }

  createHotel(body: CreateHotelDto): Observable<HotelDto> {
    return apiHotelsPost$Json(this.http, this.api.rootUrl, { body }).pipe(
      map((response) => response.body),
      unwrapApiResponse<HotelDto>()
    );
  }

  updateHotel(id: number, body: UpdateHotelDto): Observable<HotelDto> {
    return apiHotelsIdPatch$Json(this.http, this.api.rootUrl, { id, body }).pipe(
      map((response) => response.body),
      unwrapApiResponse<HotelDto>()
    );
  }

  deleteHotel(id: number, isSoft = true, forceDelete = false): Observable<string> {
    return apiHotelsIdDelete$Json(this.http, this.api.rootUrl, { id, isSoft, forceDelete }).pipe(
      map((response) => {
        const envelope = response.body;
        return envelope ? { ...envelope, data: envelope.data ?? undefined } : envelope;
      }),
      unwrapApiResponse<string>()
    );
  }
}
