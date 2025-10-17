import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError, from } from 'rxjs';
import { AuthService } from '@authentication/services/auth.service';
import { environment } from '@environment/environment';

/**
 * Authentication interceptor that handles:
 * 1. Adding JWT tokens to requests
 * 2. Automatically refreshing tokens on 401 errors
 * 3. Redirecting to login when refresh fails
 */
export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const authService = inject(AuthService);

  // Skip authentication logic for specific endpoints
  const skipPaths = [
    '/login', 
    '/refresh-token',
    '/logout-device',
    '/logout-all'
  ];
  
  const shouldSkip = skipPaths.some(path => req.url.includes(path));
  
  if (shouldSkip) {
    return next(req);
  }

  // Skip if no token exists
  const token = authService.getAuthorizationToken();
  if (!token) {
    return next(req);
  }

  // Add JWT token to request
  const authRequest = req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });

  return next(authRequest).pipe(
    catchError((error: HttpErrorResponse) => {
      // Only handle 401 Unauthorized errors
      if (error.status === 401) {
        // Check if we have a refresh token
        const refreshToken = authService.getRefreshToken();
        if (!refreshToken) {
          // No refresh token, redirect to login
          authService.clearTokens();
          window.location.href = '/login';
          return throwError(() => error);
        }

        // Try to refresh the token
        return from(authService.refreshToken()).pipe(
          switchMap((success) => {
            if (success) {
              // Token refreshed successfully, retry the original request
              const newToken = authService.getAuthorizationToken();
              if (newToken) {
                const newAuthRequest = req.clone({
                  setHeaders: {
                    Authorization: `Bearer ${newToken}`
                  }
                });
                return next(newAuthRequest);
              }
            }
            
            // Refresh failed, redirect to login
            authService.clearTokens();
            window.location.href = '/login';
            return throwError(() => error);
          }),
          catchError(() => {
            // Refresh failed, redirect to login
            authService.clearTokens();
            window.location.href = '/login';
            return throwError(() => error);
          })
        );
      }

      // For non-401 errors, just pass them through
      return throwError(() => error);
    })
  );
};
