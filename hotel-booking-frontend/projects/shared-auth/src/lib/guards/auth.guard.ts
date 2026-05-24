import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from '../store/auth.store';

export const authGuard: CanActivateFn = (route) => {
  const auth = inject(AuthStore);
  const router = inject(Router);

  if (auth.isAuthenticated()) {
    return true;
  }

  const loginPath = (route.data['loginPath'] as string | undefined) ?? '/auth/login';
  const returnUrl = router.url;
  return router.createUrlTree([loginPath], {
    queryParams: returnUrl && returnUrl !== loginPath ? { returnUrl } : undefined,
  });
};
