import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let mensagem = 'Ocorreu um erro inesperado.';

      switch (error.status) {
        case 0:
          mensagem = 'Não foi possível conectar ao servidor.';
          break;

        case 400:
          mensagem = 'Os dados informados são inválidos.';
          break;

        case 404:
          mensagem = 'O recurso solicitado não foi encontrado.';
          break;

        case 409:
          mensagem = 'Não foi possível concluir a operação devido a um conflito.';
          break;

        case 500:
          mensagem = 'Ocorreu um erro interno no servidor.';
          break;
      }

      // Utiliza uma mensagem da API quando disponível.
      const mensagemApi =
        typeof error.error === 'object' &&
        error.error !== null
          ? error.error.message ?? error.error.detail
          : null;

      if (typeof mensagemApi === 'string' && mensagemApi.trim()) {
        mensagem = mensagemApi;
      }

      snackBar.open(mensagem, 'Fechar', {
        duration: 5000,
        horizontalPosition: 'end',
        verticalPosition: 'top'
      });

      return throwError(() => error);
    })
  );
};