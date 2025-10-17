import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError, from } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { environment } from '@environment/environment';

export const refreshTokenInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const authService = inject(AuthService);

  // Skip refresh token logic for login, refresh-token, and logout endpoints
  const skipUrls = [
    `${environment.apiUrl}/login`, 
    `${environment.apiUrl}/refresh-token`,
    `${environment.apiUrl}/logout-device`,
    `${environment.apiUrl}/logout-all`
  ];
  
  const shouldSkip = skipUrls.some(url => req.url.includes(url));
  
  if (shouldSkip) {
    return next(req);
  }

  // Skip if no token exists
  const token = authService.getAuthorizationToken();
  if (!token) {
    return next(req);
  }

  return next(req).pipe(
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
                const authRequest = req.clone({
                  setHeaders: {
                    Authorization: `Bearer ${newToken}`
                  }
                });
                return next(authRequest);
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
