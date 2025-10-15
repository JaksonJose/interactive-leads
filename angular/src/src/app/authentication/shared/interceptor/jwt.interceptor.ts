import { HttpInterceptorFn, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { AuthService } from '../../services/auth.service';
import { environment } from '@environment/environment';

export const jwtInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const authService = inject(AuthService);

  const skipUrls = [`${environment.apiUrl}/login`, `${environment.apiUrl}/refresh-token`];
  const shouldSkip = skipUrls.some(url => req.url.includes(url));
  
  if (shouldSkip) {
    return next(req);
  }

  const token = authService.getAuthorizationToken();

  if (!token) {
    return next(req);
  }

  if (authService.isUserLoggedIn && !authService.isUserLoggedIn()) {
    return next(req);
  }

  const authRequest = req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });

  return next(authRequest);
};