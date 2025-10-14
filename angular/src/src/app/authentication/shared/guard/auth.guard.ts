import { inject, PLATFORM_ID } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { isPlatformBrowser } from '@angular/common';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const authService = inject(AuthService);
  const platformId = inject(PLATFORM_ID);

    // Just execute the logic in the browser
  if (!isPlatformBrowser(platformId)) {
    return false;
  }

  // Verifies whether the user is authenticated
  const isAuthenticated = authService.isUserLoggedIn();
  if (!isAuthenticated) {
    router.navigate(['/login']);
    return false;
  }
  
  // Obtain the roles requested for the route
  const requiredRoles = route.data?.['roles'] as string[] | undefined;

  // If there are defined roles, verifies if the user owns it
  if (requiredRoles && !requiredRoles.some(role => authService.hasRole(role))) {
    router.navigate(['/unauthorized']);
    return false;
  }
  
    return true;
};
