import { inject, Injectable, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

import { TokenResponse } from '@authentication/models';

@Injectable({
  providedIn: 'root'
})
export class TokenStorageService {
  private platformId = inject(PLATFORM_ID);

  /**
   * Obtém o token JWT do usuário logado
   * @returns Token armazenado no localStorage
   */
  public obterToken(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      return localStorage.getItem('token');
    }
    return null;
  }

  /**
   * Obtém o refresh token do localStorage
   * @returns Refresh token armazenado no localStorage
   */
  public obterRefreshToken(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      return localStorage.getItem('refreshToken');
    }
    return null;
  }

  /**
   * Armazena tokens no localStorage
   * @param tokenResponse Resposta contendo JWT e refresh token
   */
  public armazenarTokens(tokenResponse: TokenResponse): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem('token', tokenResponse.jwt);
      localStorage.setItem('refreshToken', tokenResponse.refreshToken);
      localStorage.setItem('refreshTokenExpiry', tokenResponse.refreshTokenExpirationDate.toString());
    }
  }

  /**
   * Limpa todos os tokens de autenticação do localStorage
   */
  public limparTokens(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem('token');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('refreshTokenExpiry');
      localStorage.removeItem('interactiveUser');
    }
  }

  /**
   * Obtém a data de expiração do refresh token
   * @returns Data de expiração ou null se não existir
   */
  public obterDataExpiracaoRefreshToken(): Date | null {
    if (isPlatformBrowser(this.platformId)) {
      const expiry = localStorage.getItem('refreshTokenExpiry');
      return expiry ? new Date(expiry) : null;
    }
    return null;
  }
}
