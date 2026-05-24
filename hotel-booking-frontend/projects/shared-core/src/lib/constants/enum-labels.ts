export type BookingStatus = 0 | 1 | 2 | 3 | 4;
export type UserRole = 0 | 1 | 2;
export type RoomType = 0 | 1 | 2 | 3;
export type PaymentStatus = 0 | 1 | 2 | 3 | 4;

export const BOOKING_STATUS_LABELS: Record<BookingStatus, string> = {
  0: 'Pending',
  1: 'Confirmed',
  2: 'Cancelled',
  3: 'Completed',
  4: 'No Show',
};

export const USER_ROLE_LABELS: Record<UserRole, string> = {
  0: 'Customer',
  1: 'Admin',
  2: 'Hotel Manager',
};

export const ROOM_TYPE_LABELS: Record<RoomType, string> = {
  0: 'Standard',
  1: 'Deluxe',
  2: 'Suite',
  3: 'Presidential',
};

export const PAYMENT_STATUS_LABELS: Record<PaymentStatus, string> = {
  0: 'Pending',
  1: 'Completed',
  2: 'Failed',
  3: 'Refunded',
  4: 'Cancelled',
};

export function bookingStatusLabel(status: BookingStatus): string {
  return BOOKING_STATUS_LABELS[status];
}

export function userRoleLabel(role: UserRole): string {
  return USER_ROLE_LABELS[role];
}

export function roomTypeLabel(type: RoomType): string {
  return ROOM_TYPE_LABELS[type];
}

export function paymentStatusLabel(status: PaymentStatus): string {
  return PAYMENT_STATUS_LABELS[status];
}
