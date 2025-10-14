import { HttpErrorResponse, HttpEvent, HttpInterceptorFn, HttpRequest, HttpResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, tap, throwError } from 'rxjs';
import { MessageService } from 'primeng/api';
import { TranslateService } from '@ngx-translate/core';

import { AuthService } from '../../services/auth.service';
import { Message } from '@core/models/message';


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
  // api conection issue
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
    const errorMessages: Array<Message>  = error.error?.messages;
    if (!errorMessages) {
      messageService.add({
        key: 'app-toast',
        severity: 'error',
        summary: translateService.instant('api.error.title'),
        detail: `${error.status}: ${error.statusText}`
      })

      return throwError(() => error);
    }

    if (errorMessages) {

      if (errorMessages[0].messageText === 'authenticationInvalid'
        || errorMessages[0].messageText === 'authenticationBlocked') {
          return throwError(() => error);
        }

      messageService.addAll(
        errorMessages.map(e => ({
          key: 'app-toast',
          severity: 'error',
          summary: translateService.instant('api.error.title'),
          detail: translateService.instant(`api.error.${e.messageText}`),
        })
      ));
    }
  }

  return throwError(() => error);
}

const handleSuccess = (event: HttpResponse<any>, messageService: MessageService, translateService: TranslateService)  => {
    const messages: Array<Message> = event.body?.messages ?? new Array<Message>();

    if (messages.length === 0) return;

    messageService.addAll(
      messages.map(m => ({
        key: 'app-toast',
        severity: 'success',
        summary: translateService.instant('api.success.title'),
        detail: translateService.instant(`api.success.${m.messageText}`),
      }))
    );
}
