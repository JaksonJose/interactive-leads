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
        // Intercepta apenas respostas HTTP bem-sucedidas
        if (event instanceof HttpResponse) {
          const response = event.body as Response<unknown>;

          // Verifica se a resposta tem mensagens para exibir
          if (response && response.messages && response.messages.length > 0) {
            response.messages.forEach((message: Message) => {
              if (message.text) {
                const messageConfig = {
                  severity: obterSeveridade(message.type),
                  summary: obterTitulo(message.type),
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
        // Trata erros HTTP
        if (error.error && error.error.messages && error.error.messages.length > 0) {
          // Se o erro tem mensagens estruturadas da API
          error.error.messages.forEach((message: Message) => {
            if (message.text) {
              const messageConfig = {
                severity: obterSeveridade(message.type) || 'error',
                summary: obterTitulo(message.type) || 'Erro',
                detail: message.text,
                life: 7000
              };

              messageService.add(messageConfig);
            }
          });
        } else {
          // Erro genérico quando não há mensagens estruturadas
          const messageConfig = {
            severity: 'error',
            summary: 'Erro',
            detail: error.message || 'Ocorreu um erro inesperado',
            life: 7000
          };

          messageService.add(messageConfig);
        }
      }
    })
  );
};

/**
 * Converte o tipo de mensagem da API para a severidade do PrimeNG
 */
function obterSeveridade(messageType?: MessageType): string {
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
 * Obtém o título apropriado para cada tipo de mensagem
 */
function obterTitulo(messageType?: MessageType): string {
  switch (messageType) {
    case MessageType.success:
      return 'Sucesso';
    case MessageType.error:
      return 'Erro';
    case MessageType.warn:
      return 'Atenção';
    case MessageType.info:
      return 'Informação';
    default:
      return 'Informação';
  }
}
