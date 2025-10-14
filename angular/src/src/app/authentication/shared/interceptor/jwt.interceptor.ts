import { HttpErrorResponse, HttpEvent, HttpInterceptorFn, HttpRequest, HttpResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, tap, throwError } from 'rxjs';
import { MessageService } from 'primeng/api';
import { TranslateService } from '@ngx-translate/core';

import { AuthService } from '../../services/auth.service';
import { BaseResponseWrapper, GenericResponseWrapper } from '../../models/response-wrapper';


export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const messageService = inject(MessageService);
  const translateService = inject(TranslateService);

  const token = authService.getAuthorizationToken();

  let request: HttpRequest<any> = req;

  if (token) {
    request = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    })
  }

  return next(request).pipe(
    tap((event) => {
      if (event instanceof HttpResponse)
        handleSuccess(event, messageService, translateService)
    }),
    catchError((error) => handleError(error, messageService, translateService))
  );
};

const handleError = (error: HttpErrorResponse, messageService: MessageService, translateService: TranslateService) => {
  // API connection issue
  if (error.status === 0) {
    messageService.add({
      key: 'app-toast',
      severity: 'error',
      summary: translateService.instant('api.error.title'),
      detail: translateService.instant('api.error.unavailable')
    });

    return throwError(() => error);
  }

  // The service is temporarily unavailable
  if (error.status === 503) {
    messageService.add({
      key: 'app-toast',
      severity: 'error',
      summary: translateService.instant('api.error.title'),
      detail: translateService.instant('api.error.serviceUnavailable')
    });

    return throwError(() => error);
  }

  // timeout
  if (error.status === 504) {
    messageService.add({
      key: 'app-toast',
      severity: 'error',
      summary: translateService.instant('api.error.timeout'),
      detail: translateService.instant('api.error.timeoutMessage')
    });

    return throwError(() => error);
  }

  if (error.error instanceof ErrorEvent) {
    //Show message
    messageService.add({
      key: 'app-toast',
      severity: 'error',
      summary: translateService.instant('api.error.title'),
      detail: translateService.instant(error.error.message)
    })
  }
  else {
    const errorResponse: BaseResponseWrapper = error.error;
    
    if (!errorResponse || !errorResponse.messages) {
      messageService.add({
        key: 'app-toast',
        severity: 'error',
        summary: translateService.instant('api.error.title'),
        detail: `${error.status}: ${error.statusText}`
      })

      return throwError(() => error);
    }

    if (errorResponse.messages.length > 0) {
      // Check if it's an authentication error to avoid showing toast
      if (errorResponse.messages[0] === 'authenticationInvalid'
        || errorResponse.messages[0] === 'authenticationBlocked') {
          return throwError(() => error);
        }

      messageService.addAll(
        errorResponse.messages.map(message => ({
          key: 'app-toast',
          severity: 'error',
          summary: translateService.instant('api.error.title'),
          detail: translateService.instant(`api.error.${message}`),
        })
      ));
    }
  }

  return throwError(() => error);
}

const handleSuccess = (event: HttpResponse<unknown>, messageService: MessageService, translateService: TranslateService)  => {
    const response: BaseResponseWrapper = event.body as BaseResponseWrapper;

    if (!response || !response.messages || response.messages.length === 0) return;

    messageService.addAll(
      response.messages.map(message => ({
        key: 'app-toast',
        severity: 'success',
        summary: translateService.instant('api.success.title'),
        detail: translateService.instant(`api.success.${message}`),
      }))
    );
}
