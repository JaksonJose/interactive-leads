import { HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { MessageService } from 'primeng/api';
import { tap } from 'rxjs/operators';
import { Response, Message, MessageType } from '@core/responses';

export const apiResponseInterceptor: HttpInterceptorFn = (req, next) => {
  const messageService = inject(MessageService);

  return next(req).pipe(
    tap({
      next: (event) => {
        // Intercept only successful HTTP responses
        if (event instanceof HttpResponse) {
          const response = event.body as Response<unknown>;

          // Check if the response has messages to display
          if (response && response.messages && response.messages.length > 0) {
            response.messages.forEach((message: Message) => {
              if (message.text) {
                const messageConfig = {
                  severity: getSeverity(message.type),
                  summary: getTitle(message.type),
                  detail: message.text,
                  life: 5000
                };

                messageService.add(messageConfig);
              }
            });
          }
        }
      },
      error: (error) => {
        // Handle HTTP errors
        if (error.error && error.error.messages && error.error.messages.length > 0) {
          // If the error has structured messages from the API
          error.error.messages.forEach((message: Message) => {
            if (message.text) {
              const messageConfig = {
                severity: getSeverity(message.type) || 'error',
                summary: getTitle(message.type) || 'Error',
                detail: message.text,
                life: 7000
              };

              messageService.add(messageConfig);
            }
          });
        } else {
          // Generic error when there are no structured messages
          const messageConfig = {
            severity: 'error',
            summary: 'Error',
            detail: error.message || 'An unexpected error occurred',
            life: 7000
          };

          messageService.add(messageConfig);
        }
      }
    })
  );
};

/**
 * Converts the API message type to PrimeNG severity
 */
function getSeverity(messageType?: MessageType): string {
  switch (messageType) {
    case MessageType.success:
      return 'success';
    case MessageType.error:
      return 'error';
    case MessageType.warn:
      return 'warn';
    case MessageType.info:
      return 'info';
    default:
      return 'info';
  }
}

/**
 * Gets the appropriate title for each message type
 */
function getTitle(messageType?: MessageType): string {
  switch (messageType) {
    case MessageType.success:
      return 'Success';
    case MessageType.error:
      return 'Error';
    case MessageType.warn:
      return 'Warning';
    case MessageType.info:
      return 'Information';
    default:
      return 'Information';
  }
}
