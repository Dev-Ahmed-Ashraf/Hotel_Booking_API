import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Api } from '../generated/api';
import {
  apiRoomsAvailableGet$Json,
  apiRoomsGet$Json,
  apiRoomsIdDelete$Json,
  apiRoomsIdGet$Json,
  apiRoomsIdPatch$Json,
  apiRoomsPost$Json,
} from '../generated/functions';
import type { ApiRoomsAvailableGet$Json$Params } from '../generated/fn/rooms/api-rooms-available-get-json';
import type { ApiRoomsGet$Json$Params } from '../generated/fn/rooms/api-rooms-get-json';
import type { CreateRoomDto } from '../generated/models/create-room-dto';
import type { RoomDto } from '../generated/models/room-dto';
import type { RoomDtoPagedList } from '../generated/models/room-dto-paged-list';
import type { UpdateRoomDto } from '../generated/models/update-room-dto';
import { unwrapApiResponse, type ApiEnvelope } from '../operators/unwrap-api-response.operator';

function normalizeEnvelope<T>(envelope: ApiEnvelope<T | null> | null | undefined): ApiEnvelope<T> | null | undefined {
  if (!envelope || envelope.data === null) {
    return envelope ? { ...envelope, data: envelope.data ?? undefined } : envelope;
  }
  return envelope as ApiEnvelope<T>;
}

export interface RoomTypeOption {
  id: number;
  name: string;
}

@Injectable({ providedIn: 'root' })
export class RoomsApiFacade {
  private readonly api = inject(Api);
  private readonly http = inject(HttpClient);

  getRooms(params?: ApiRoomsGet$Json$Params): Observable<RoomDtoPagedList> {
    return apiRoomsGet$Json(this.http, this.api.rootUrl, params).pipe(
      map((response) => response.body),
      unwrapApiResponse<RoomDtoPagedList>()
    );
  }

  getRoomsByHotel(hotelId: number, params?: Omit<ApiRoomsGet$Json$Params, 'hotelId'>): Observable<RoomDtoPagedList> {
    return this.getRooms({ ...params, hotelId });
  }

  getRoomById(id: number): Observable<RoomDto> {
    return apiRoomsIdGet$Json(this.http, this.api.rootUrl, { id }).pipe(
      map((response) => response.body),
      unwrapApiResponse<RoomDto>()
    );
  }

  createRoom(body: CreateRoomDto): Observable<RoomDto> {
    return apiRoomsPost$Json(this.http, this.api.rootUrl, { body }).pipe(
      map((response) => response.body),
      unwrapApiResponse<RoomDto>()
    );
  }

  updateRoom(id: number, body: UpdateRoomDto): Observable<RoomDto> {
    return apiRoomsIdPatch$Json(this.http, this.api.rootUrl, { id, body }).pipe(
      map((response) => response.body),
      unwrapApiResponse<RoomDto>()
    );
  }

  deleteRoom(id: number, isSoft = true, forceDelete = false): Observable<string> {
    return apiRoomsIdDelete$Json(this.http, this.api.rootUrl, { id, isSoft, forceDelete }).pipe(
      map((response) => {
        const envelope = response.body;
        return envelope ? { ...envelope, data: envelope.data ?? undefined } : envelope;
      }),
      unwrapApiResponse<string>()
    );
  }

  getAvailableRooms(params: ApiRoomsAvailableGet$Json$Params): Observable<RoomDto[]> {
    return apiRoomsAvailableGet$Json(this.http, this.api.rootUrl, params).pipe(
      map((response) => normalizeEnvelope(response.body)),
      unwrapApiResponse<RoomDto[]>()
    );
  }

  getRoomTypes(): Observable<RoomTypeOption[]> {
    return this.http
      .get<{ success?: boolean; data?: RoomTypeOption[] }>(`${this.api.rootUrl}/Rooms/types`)
      .pipe(
        map((response) => {
          if (response?.data) {
            return response.data;
          }
          if (Array.isArray(response)) {
            return response as RoomTypeOption[];
          }
          return [];
        })
      );
  }
}
