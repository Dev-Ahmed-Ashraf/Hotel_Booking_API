import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { jwtDecode } from 'jwt-decode';
import type { AuthResponseDto } from '@hotel/shared/data-access';
import type { UserDto } from '@hotel/shared/data-access';
import type { UserRole } from '@hotel/shared/data-access';
import { TOKEN_STORAGE_KEY } from '../constants/token-storage.key';

const USER_STORAGE_KEY = 'hotel-auth-user';

interface JwtPayload {
  exp?: number;
  role?: string;
}

function parseRole(value: string | undefined): UserRole | null {
  switch (value) {
    case 'Customer':
      return 0;
    case 'Admin':
      return 1;
    case 'HotelManager':
      return 2;
    default:
      return null;
  }
}

@Injectable({ providedIn: 'root' })
export class AuthStore {
  private readonly tokenState = signal<string | null>(this.readToken());
  private readonly userState = signal<UserDto | null>(this.readUser());

  readonly token = this.tokenState.asReadonly();
  readonly user = this.userState.asReadonly();
  readonly isAuthenticated = computed(() => !!this.tokenState());
  readonly role = computed<UserRole | null>(() => this.userState()?.role ?? this.roleFromToken());
  readonly expiresAt = computed<Date | null>(() => {
    const fromUser = this.expiresAtFromStorage();
    if (fromUser) {
      return fromUser;
    }
    const token = this.tokenState();
    if (!token) {
      return null;
    }
    try {
      const payload = jwtDecode<JwtPayload>(token);
      return payload.exp ? new Date(payload.exp * 1000) : null;
    } catch {
      return null;
    }
  });

  constructor() {
    effect(() => {
      const token = this.tokenState();
      if (token) {
        localStorage.setItem(TOKEN_STORAGE_KEY, token);
      } else {
        localStorage.removeItem(TOKEN_STORAGE_KEY);
      }
    });

    effect(() => {
      const user = this.userState();
      if (user) {
        sessionStorage.setItem(USER_STORAGE_KEY, JSON.stringify(user));
      } else {
        sessionStorage.removeItem(USER_STORAGE_KEY);
      }
    });
  }

  setSession(response: AuthResponseDto): void {
    this.tokenState.set(response.token ?? null);
    this.userState.set(response.user ?? null);
    if (response.expiresAt) {
      sessionStorage.setItem('hotel-auth-expires', response.expiresAt);
    }
  }

  clearSession(): void {
    this.tokenState.set(null);
    this.userState.set(null);
    sessionStorage.removeItem('hotel-auth-expires');
  }

  private readToken(): string | null {
    return localStorage.getItem(TOKEN_STORAGE_KEY);
  }

  private readUser(): UserDto | null {
    const raw = sessionStorage.getItem(USER_STORAGE_KEY);
    if (!raw) {
      return null;
    }
    try {
      return JSON.parse(raw) as UserDto;
    } catch {
      return null;
    }
  }

  private roleFromToken(): UserRole | null {
    const token = this.tokenState();
    if (!token) {
      return null;
    }
    try {
      const payload = jwtDecode<JwtPayload>(token);
      return parseRole(payload.role);
    } catch {
      return null;
    }
  }

  private expiresAtFromStorage(): Date | null {
    const raw = sessionStorage.getItem('hotel-auth-expires');
    return raw ? new Date(raw) : null;
  }
}
