import { Routes } from '@angular/router';

import { authGuard, guestGuard } from '@hotel/shared/auth';



export const routes: Routes = [

  {

    path: '',

    loadComponent: () =>

      import('@hotel/shared/ui').then((m) => m.AppShellComponent),

    data: { titleKey: 'app.customerPortal', showNav: true },

    children: [

      {

        path: '',

        loadComponent: () =>

          import('./features/home/home.component').then((m) => m.HomeComponent),

      },

      {

        path: 'hotels',

        loadComponent: () =>

          import('./features/hotels/hotel-list.component').then((m) => m.HotelListComponent),

      },

      {

        path: 'hotels/:id',

        loadComponent: () =>

          import('./features/hotels/hotel-detail.component').then((m) => m.HotelDetailComponent),

      },

      {

        path: 'booking',

        canActivate: [authGuard],

        loadComponent: () =>

          import('./features/booking/booking-wizard.component').then((m) => m.BookingWizardComponent),

      },

      {

        path: 'booking/confirmation/:id',

        loadComponent: () =>

          import('./features/booking/booking-confirmation.component').then(

            (m) => m.BookingConfirmationComponent

          ),

      },

      {

        path: 'my-bookings',

        canActivate: [authGuard],

        loadComponent: () =>

          import('./features/my-bookings/my-bookings-list.component').then(

            (m) => m.MyBookingsListComponent

          ),

      },

      {

        path: 'my-bookings/:id',

        canActivate: [authGuard],

        loadComponent: () =>

          import('./features/my-bookings/my-booking-detail.component').then(

            (m) => m.MyBookingDetailComponent

          ),

      },

      {

        path: 'my-reviews',

        canActivate: [authGuard],

        loadComponent: () =>

          import('./features/reviews/my-reviews-list.component').then(

            (m) => m.MyReviewsListComponent

          ),

      },

    ],

  },

  {

    path: 'auth',

    canActivate: [guestGuard],

    children: [

      {

        path: 'login',

        loadComponent: () =>

          import('./features/auth/login/login.component').then((m) => m.LoginComponent),

      },

      {

        path: 'register',

        loadComponent: () =>

          import('./features/auth/register/register.component').then(

            (m) => m.RegisterComponent

          ),

      },

    ],

  },

  { path: '**', redirectTo: '' },

];


