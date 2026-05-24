import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Api } from '../generated/api';
import {
  apiBookingsBookingIdPaymentGet$Json,
  apiPaymentsIntentsPost$Json,
} from '../generated/functions';
import type { CreatePaymentIntentResponseDto } from '../generated/models/create-payment-intent-response-dto';
import type { PaymentDto } from '../generated/models/payment-dto';
import { unwrapApiResponse } from '../operators/unwrap-api-response.operator';

@Injectable({ providedIn: 'root' })
export class PaymentsApiFacade {
  private readonly api = inject(Api);
  private readonly http = inject(HttpClient);

  createPaymentIntent(bookingId: number): Observable<CreatePaymentIntentResponseDto> {
    return apiPaymentsIntentsPost$Json(this.http, this.api.rootUrl, {
      body: { bookingId },
    }).pipe(
      map((response) => response.body),
      unwrapApiResponse<CreatePaymentIntentResponseDto>()
    );
  }

  getPaymentByBooking(bookingId: number): Observable<PaymentDto> {
    return apiBookingsBookingIdPaymentGet$Json(this.http, this.api.rootUrl, { bookingId }).pipe(
      map((response) => response.body),
      unwrapApiResponse<PaymentDto>()
    );
  }
}
