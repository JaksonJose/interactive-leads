import { inject, Injectable } from '@angular/core';
import { jwtDecode, JwtPayload } from "jwt-decode";

import { Response } from '@core/responses/response';
import { LoginModel, TokenResponse } from '@authentication/models';
import { Observable } from 'rxjs';
import { AuthRepository } from '../repositories/auth.repository';
import { TokenStorageService } from './token-storage.service';

interface ExtendedJwtPayload extends JwtPayload {
  role?: string | string[];
  permission?: string | string[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private authRepository = inject(AuthRepository);
  private tokenStorage = inject(TokenStorageService);

  public AuthenticateUser(login: LoginModel): Observable<Response<TokenResponse>> {
    return this.authRepository.autenticarUsuario(login);
  }

  /**
   * Obtain the JWT token of the logged user
   * @returns Get the token stored in local storage
   */
  public getAuthorizationToken(): string | null {
    return this.tokenStorage.obterToken();
  }

  /**
   * Get refresh token from local storage
   * @returns The refresh token stored in local storage
   */
  public getRefreshToken(): string | null {
    return this.tokenStorage.obterRefreshToken();
  }

  /**
   * Store tokens in local storage
   * @param tokenResponse Token response containing JWT and refresh token
   */
  public storeTokens(tokenResponse: TokenResponse): void {
    this.tokenStorage.armazenarTokens(tokenResponse);
  }

  /**
   * Clear all authentication tokens from local storage
   */
  public clearTokens(): void {
    this.tokenStorage.limparTokens();
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
   * Get user permissions from JWT token
   * @returns Array of user permissions
   */
  public getUserPermissions(): string[] {
    const token = this.getAuthorizationToken();
    if (!token) return [];
    
    const decoded = jwtDecode<ExtendedJwtPayload>(token);
    return decoded?.permission ? 
      (Array.isArray(decoded.permission) ? decoded.permission : [decoded.permission]) : 
      [];
  }

  /**
   * Check if user has a specific permission
   * @param permission The permission to check
   * @returns True if user has the permission
   */
  public hasPermission(permission: string): boolean {
    return this.getUserPermissions().includes(permission);
  }

  /**
   * Check if user has any of the specified permissions
   * @param permissions Array of permissions to check
   * @returns True if user has at least one of the permissions
   */
  public hasAnyPermission(permissions: string[]): boolean {
    return permissions.some(permission => this.hasPermission(permission));
  }

  /**
   * Check if user has all of the specified permissions
   * @param permissions Array of permissions to check
   * @returns True if user has all of the permissions
   */
  public hasAllPermissions(permissions: string[]): boolean {
    return permissions.every(permission => this.hasPermission(permission));
  }

  /**
   * Check if user is a root administrator (has tenant management permissions)
   * @returns True if user has root admin permissions
   */
  public isRootAdmin(): boolean {
    const rootPermissions = [
      'Permission.Tenants.Create',
      'Permission.Tenants.Read',
      'Permission.Tenants.Update'
    ];
    return this.hasAllPermissions(rootPermissions);
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
