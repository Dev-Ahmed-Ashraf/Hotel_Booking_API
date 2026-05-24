import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import type { UserRole } from '@hotel/shared/data-access';
import { AuthStore } from '../store/auth.store';

export const roleGuard: CanActivateFn = (route) => {
  const auth = inject(AuthStore);
  const router = inject(Router);
  const allowed = (route.data['roles'] as UserRole[] | undefined) ?? [];

  if (!auth.isAuthenticated()) {
    const loginPath = (route.data['loginPath'] as string | undefined) ?? '/auth/login';
    return router.createUrlTree([loginPath]);
  }

  const role = auth.role();
  if (allowed.length === 0 || (role !== null && allowed.includes(role))) {
    return true;
  }

  return router.createUrlTree(['/']);
};
