import { HttpClient } from '@angular/common/http';
import { inject, Injectable, PLATFORM_ID } from '@angular/core';
import { jwtDecode, JwtPayload } from "jwt-decode";

interface ExtendedJwtPayload extends JwtPayload {
  role?: string | string[];
}

import { LoginModel, TokenResponse, LoginResponseWrapper, RefreshTokenResponseWrapper, RegisterModel } from '../models';
import { isPlatformBrowser } from '@angular/common';
import { environment } from '@environment/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private baseUrl = `${environment.apiUrl}/${environment.apiVersion}/token`;

  private http = inject(HttpClient);
  private platformId = inject(PLATFORM_ID);

  public AuthenticateUser(login: LoginModel): Observable<LoginResponseWrapper> {
    return this.http.post<LoginResponseWrapper>(`${this.baseUrl}/login`, login, { withCredentials: false });
  }

  public createAuthenticationAsync(registerModel: RegisterModel) : Observable<unknown>  {
    return this.http.post(`${this.baseUrl}/register`, registerModel);
  }

  public updateCuthenticationAndConsultant(registerModel: RegisterModel): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/update`, registerModel);
  }

  public refreshToken(refreshToken: string): Observable<RefreshTokenResponseWrapper> {
    return this.http.post<RefreshTokenResponseWrapper>(`${this.baseUrl}/refresh-token`, { currentJwt: refreshToken });
  }

  /**
   * Obtain the JWT token of the logged user
   * @returns Get the token stored in local storage
   */
  public getAuthorizationToken(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      return localStorage.getItem('token');
    }

    return null;
  }

  /**
   * Get refresh token from local storage
   * @returns The refresh token stored in local storage
   */
  public getRefreshToken(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      return localStorage.getItem('refreshToken');
    }

    return null;
  }

  /**
   * Store tokens in local storage
   * @param tokenResponse Token response containing JWT and refresh token
   */
  public storeTokens(tokenResponse: TokenResponse): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem('token', tokenResponse.jwt);
      localStorage.setItem('refreshToken', tokenResponse.refreshToken);
      localStorage.setItem('refreshTokenExpiry', tokenResponse.refreshTokenExpirationDate.toString());
    }
  }

  /**
   * Clear all authentication tokens from local storage
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
   * Get roles from JWT token
   * @returns Array of user roles
   */
   public getUserRoles(): string[] {
    const token = this.getAuthorizationToken();
    if (!token) return [];
    
    const decoded = jwtDecode<ExtendedJwtPayload>(token);

    return decoded?.role ? (Array.isArray(decoded.role) ? decoded.role : [decoded.role]) : [];
  }

  /**
   * Check if user has a specific role
   * @param role The role to check
   * @returns True if user has the role
   */
    public hasRole(role: string): boolean {
      return this.getUserRoles().includes(role);
    }

  /**
   * Verify if user is authenticated
   * @returns True if user is logged in and token is valid
   */
  public isUserLoggedIn(): boolean {
    const token = this.getAuthorizationToken();

    if (!token || this.isTokenExpired(token)) return false;

    return true;
  }

  /**
   * Verify if the user is a consultant
   * @returns True if user has consultant role
   */
  public isConsultant() : boolean {
    const roles = this.getUserRoles();
    return roles.includes('Consultant');
  }

  /**
   * Verify if the user is a manager
   * @returns True if user has manager role
   */
  public isManager() : boolean {
      const roles = this.getUserRoles();
      return roles.includes('Manager');
    }

  /**
   * Verify if the user is an owner
   * @returns True if user has owner role
   */
  public isOwner(): boolean {
    const roles = this.getUserRoles();
    return roles.includes('Owner');
  }

  /**
   * Verify if the user has support role
   * @returns True if user has support role
   */
  public isSupport() : boolean {
    const roles = this.getUserRoles();
    return roles.includes('Support');
  }

  /**
   * Verify if the user is a system administrator
   * @returns True if user has sysadmin role
   */
  public isSysAdmin() : boolean {
    const roles = this.getUserRoles();
    return roles.includes('SysAdmin');
  }

  /**
   * Get token expiration date
   * @param token The JWT token to decode
   * @returns Expiration date or null if invalid
   */
   private getTokenExpirationDate(token: string): Date | null {
    try {
      const decoded = jwtDecode<JwtPayload>(token);

      if (!decoded.exp) return null;

      const date = new Date(0);
      date.setUTCSeconds(decoded.exp);

      return date;
    } catch (ex) {
      console.error(`getTokenExpirationDate method Error: ${ex}`);
      return null;
    }
  }

  /**
   * Verify if token is valid and not expired
   * @param token The JWT token to validate
   * @returns True if token is valid and not expired
   */
  private isTokenExpired(token?: string): boolean {
    if (!token) return true;

    const date = this.getTokenExpirationDate(token);
    if (!date) return true;

    return !(date.valueOf() > new Date().valueOf());
  }
}
