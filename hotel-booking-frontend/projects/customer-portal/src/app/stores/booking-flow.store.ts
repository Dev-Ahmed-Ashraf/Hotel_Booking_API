import { Injectable, signal } from '@angular/core';
import type { BookingDto } from '@hotel/shared/data-access';
import type { BookingPriceResponseDto } from '@hotel/shared/data-access';

export interface BookingFlowParams {
  hotelId: number;
  roomId: number;
  checkIn: string;
  checkOut: string;
}

@Injectable({ providedIn: 'root' })
export class BookingFlowStore {
  readonly params = signal<BookingFlowParams | null>(null);
  readonly pricePreview = signal<BookingPriceResponseDto | null>(null);
  readonly booking = signal<BookingDto | null>(null);
  readonly clientSecret = signal<string | null>(null);

  setParams(params: BookingFlowParams): void {
    this.params.set(params);
    this.pricePreview.set(null);
    this.booking.set(null);
    this.clientSecret.set(null);
  }

  reset(): void {
    this.params.set(null);
    this.pricePreview.set(null);
    this.booking.set(null);
    this.clientSecret.set(null);
  }
}
