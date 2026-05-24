import { Routes } from '@angular/router';
import { authGuard, guestGuard, roleGuard } from '@hotel/shared/auth';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/login/login.component').then((m) => m.AdminLoginComponent),
  },
  {
    path: '',
    canActivate: [authGuard],
    data: {
      loginPath: '/login',
      titleKey: 'app.adminDashboard',
      showNav: true,
      adminNav: true,
    },
    loadComponent: () =>
      import('@hotel/shared/ui').then((m) => m.AppShellComponent),
    children: [
      {
        path: '',
        pathMatch: 'full',
        loadComponent: () =>
          import('./features/home/admin-home-redirect.component').then(
            (m) => m.AdminHomeRedirectComponent
          ),
      },
      {
        path: 'dashboard',
        canActivate: [roleGuard],
        data: { roles: [1], loginPath: '/login' },
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent),
      },
      {
        path: 'hotels',
        canActivate: [roleGuard],
        data: { roles: [1, 2], loginPath: '/login' },
        loadComponent: () =>
          import('./features/hotels/hotel-list.component').then((m) => m.HotelListComponent),
      },
      {
        path: 'hotels/new',
        canActivate: [roleGuard],
        data: { roles: [1], loginPath: '/login' },
        loadComponent: () =>
          import('./features/hotels/hotel-form.component').then((m) => m.HotelFormComponent),
      },
      {
        path: 'hotels/:id/edit',
        canActivate: [roleGuard],
        data: { roles: [1, 2], loginPath: '/login' },
        loadComponent: () =>
          import('./features/hotels/hotel-form.component').then((m) => m.HotelFormComponent),
      },
      {
        path: 'rooms',
        canActivate: [roleGuard],
        data: { roles: [1, 2], loginPath: '/login' },
        loadComponent: () =>
          import('./features/rooms/room-list.component').then((m) => m.RoomListComponent),
      },
      {
        path: 'bookings',
        canActivate: [roleGuard],
        data: { roles: [1, 2], loginPath: '/login' },
        loadComponent: () =>
          import('./features/bookings/booking-list.component').then((m) => m.BookingListComponent),
      },
      {
        path: 'reviews',
        canActivate: [roleGuard],
        data: { roles: [1], loginPath: '/login' },
        loadComponent: () =>
          import('./features/reviews/review-list.component').then((m) => m.ReviewListComponent),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
