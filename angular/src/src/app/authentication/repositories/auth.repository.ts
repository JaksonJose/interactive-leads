import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { Response } from '@core/responses/response';
import { LoginModel, TokenResponse } from '@authentication/models';
import { environment } from '@environment/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthRepository {
  private baseUrl = `${environment.apiUrl}/${environment.apiVersion}/token`;
  private http = inject(HttpClient);

  /**
   * Autentica o usuário na API
   * @param login Dados de login do usuário
   * @returns Observable com resposta contendo tokens
   */
  public autenticarUsuario(login: LoginModel): Observable<Response<TokenResponse>> {
    return this.http.post<Response<TokenResponse>>(`${this.baseUrl}/login`, login, { withCredentials: false });
  }
}
