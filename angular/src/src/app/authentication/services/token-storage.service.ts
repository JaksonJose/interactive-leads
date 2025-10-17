import { inject, Injectable, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

import { TokenResponse } from '@authentication/models';

@Injectable({
  providedIn: 'root'
})
export class TokenStorageService {
  private platformId = inject(PLATFORM_ID);

  /**
   * Gets the JWT token of the logged user
   * @returns Token stored in localStorage
   */
  public getToken(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      return localStorage.getItem('token');
    }
    return null;
  }

  /**
   * Gets the refresh token from localStorage
   * @returns Refresh token stored in localStorage
   */
  public getRefreshToken(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      return localStorage.getItem('refreshToken');
    }
    return null;
  }

  /**
   * Stores tokens in localStorage
   * @param tokenResponse Response containing JWT and refresh token
   */
  public storeTokens(tokenResponse: TokenResponse): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem('token', tokenResponse.jwt);
      localStorage.setItem('refreshToken', tokenResponse.refreshToken);
      localStorage.setItem('refreshTokenExpiry', tokenResponse.refreshTokenExpirationDate.toString());
    }
  }

  /**
   * Clears all authentication tokens from localStorage
   */
  public clearTokens(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem('token');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('refreshTokenExpiry');
      localStorage.removeItem('interactiveUser');
    }
  }

  /**
   * Gets the expiration date of the refresh token
   * @returns Expiration date or null if it doesn't exist
   */
  public getRefreshTokenExpirationDate(): Date | null {
    if (isPlatformBrowser(this.platformId)) {
      const expiry = localStorage.getItem('refreshTokenExpiry');
      return expiry ? new Date(expiry) : null;
    }
    return null;
  }
}
