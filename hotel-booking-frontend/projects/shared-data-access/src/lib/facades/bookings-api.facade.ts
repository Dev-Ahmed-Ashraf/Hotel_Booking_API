import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Api } from '../generated/api';
import {
  apiBookingsCalculatePriceGet$Json,
  apiBookingsCheckAvailabilityGet$Json,
  apiBookingsGet$Json,
  apiBookingsHotelHotelIdGet$Json,
  apiBookingsIdCancelPost$Json,
  apiBookingsIdGet$Json,
  apiBookingsIdPatch$Json,
  apiBookingsIdStatusPatch$Json,
  apiBookingsPost$Json,
  apiBookingsUserUserIdGet$Json,
} from '../generated/functions';
import type { ApiBookingsCalculatePriceGet$Json$Params } from '../generated/fn/bookings/api-bookings-calculate-price-get-json';
import type { ApiBookingsCheckAvailabilityGet$Json$Params } from '../generated/fn/bookings/api-bookings-check-availability-get-json';
import type { ApiBookingsGet$Json$Params } from '../generated/fn/bookings/api-bookings-get-json';
import type { BookingDto } from '../generated/models/booking-dto';
import type { BookingDtoPagedList } from '../generated/models/booking-dto-paged-list';
import type { BookingPriceResponseDto } from '../generated/models/booking-price-response-dto';
import type { BookingStatus } from '../generated/models/booking-status';
import type { CancelBookingDto } from '../generated/models/cancel-booking-dto';
import type { ChangeBookingStatusDto } from '../generated/models/change-booking-status-dto';
import type { CreateBookingDto } from '../generated/models/create-booking-dto';
import type { UpdateBookingDto } from '../generated/models/update-booking-dto';
import { unwrapApiResponse } from '../operators/unwrap-api-response.operator';

@Injectable({ providedIn: 'root' })
export class BookingsApiFacade {
  private readonly api = inject(Api);
  private readonly http = inject(HttpClient);

  getBookings(params?: ApiBookingsGet$Json$Params): Observable<BookingDtoPagedList> {
    return apiBookingsGet$Json(this.http, this.api.rootUrl, params).pipe(
      map((response) => response.body),
      unwrapApiResponse<BookingDtoPagedList>()
    );
  }

  getBookingsByHotel(hotelId: number, page = 1, pageSize = 10): Observable<BookingDtoPagedList> {
    return apiBookingsHotelHotelIdGet$Json(this.http, this.api.rootUrl, {
      hotelId,
      pageNumber: page,
      pageSize,
    }).pipe(
      map((response) => response.body),
      unwrapApiResponse<BookingDtoPagedList>()
    );
  }

  changeBookingStatus(id: number, status: BookingStatus, notes?: string): Observable<BookingDto> {
    const body: ChangeBookingStatusDto = { status, notes };
    return apiBookingsIdStatusPatch$Json(this.http, this.api.rootUrl, { id, body }).pipe(
      map((response) => response.body),
      unwrapApiResponse<BookingDto>()
    );
  }

  createBooking(userId: number, body: CreateBookingDto): Observable<BookingDto> {
    return apiBookingsPost$Json(this.http, this.api.rootUrl, { userId, body }).pipe(
      map((response) => response.body),
      unwrapApiResponse<BookingDto>()
    );
  }

  getBookingById(id: number): Observable<BookingDto> {
    return apiBookingsIdGet$Json(this.http, this.api.rootUrl, { id }).pipe(
      map((response) => response.body),
      unwrapApiResponse<BookingDto>()
    );
  }

  getUserBookings(userId: number, page = 1, pageSize = 10): Observable<BookingDtoPagedList> {
    return apiBookingsUserUserIdGet$Json(this.http, this.api.rootUrl, {
      userId,
      pageNumber: page,
      pageSize,
    }).pipe(
      map((response) => response.body),
      unwrapApiResponse<BookingDtoPagedList>()
    );
  }

  cancelBooking(id: number, reason: string): Observable<string> {
    const body: CancelBookingDto = { reason };
    return apiBookingsIdCancelPost$Json(this.http, this.api.rootUrl, { id, body }).pipe(
      map((response) => {
        const envelope = response.body;
        return envelope ? { ...envelope, data: envelope.data ?? undefined } : envelope;
      }),
      unwrapApiResponse<string>()
    );
  }

  checkAvailability(params: ApiBookingsCheckAvailabilityGet$Json$Params): Observable<boolean> {
    return apiBookingsCheckAvailabilityGet$Json(this.http, this.api.rootUrl, params).pipe(
      map((response) => response.body),
      unwrapApiResponse<boolean>()
    );
  }

  calculatePrice(params: ApiBookingsCalculatePriceGet$Json$Params): Observable<BookingPriceResponseDto> {
    return apiBookingsCalculatePriceGet$Json(this.http, this.api.rootUrl, params).pipe(
      map((response) => response.body),
      unwrapApiResponse<BookingPriceResponseDto>()
    );
  }

  updateBooking(id: number, body: UpdateBookingDto): Observable<BookingDto> {
    return apiBookingsIdPatch$Json(this.http, this.api.rootUrl, { id, body }).pipe(
      map((response) => response.body),
      unwrapApiResponse<BookingDto>()
    );
  }
}
