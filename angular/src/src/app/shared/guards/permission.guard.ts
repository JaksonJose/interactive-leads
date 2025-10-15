import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '@authentication/services/auth.service';

/**
 * Guard that checks if the user has the required permissions to access a route.
 * Redirects to login if not authenticated or to unauthorized if missing permissions.
 */
export const permissionGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const authService = inject(AuthService);

  // Check if user is authenticated
  if (!authService.isUserLoggedIn()) {
    router.navigate(['/login']);
    return false;
  }

  // Get required permissions from route data
  const requiredPermissions = route.data?.['permissions'] as string[] | undefined;

  if (requiredPermissions && requiredPermissions.length > 0) {
    // Check if user has any of the required permissions
    const hasPermission = authService.hasAnyPermission(requiredPermissions);

    if (!hasPermission) {
      router.navigate(['/unauthorized']);
      return false;
    }
  }

  return true;
};
