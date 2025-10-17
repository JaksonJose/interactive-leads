import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { Response } from '@core/responses/response';
import { LoginModel, TokenResponse, RefreshTokenRequest, LogoutDeviceRequest } from '@authentication/models';
import { environment } from '@environment/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthRepository {
  private baseUrl = `${environment.apiUrl}/${environment.apiVersion}/token`;
  private http = inject(HttpClient);

  /**
   * Authenticates the user in the API
   * @param login User login data
   * @returns Observable with response containing tokens
   */
  public authenticateUser(login: LoginModel): Observable<Response<TokenResponse>> {
    return this.http.post<Response<TokenResponse>>(`${this.baseUrl}/login`, login, { withCredentials: false });
  }

  /**
   * Refreshes the JWT token using the refresh token
   * @param refreshRequest Request containing current tokens
   * @returns Observable with new tokens
   */
  public refreshToken(refreshRequest: RefreshTokenRequest): Observable<Response<TokenResponse>> {
    return this.http.post<Response<TokenResponse>>(`${this.baseUrl}/refresh-token`, refreshRequest, { withCredentials: false });
  }

  /**
   * Logout from current device only
   * @param logoutRequest Request containing refresh token
   * @returns Observable with logout result
   */
  public logoutFromCurrentDevice(logoutRequest: LogoutDeviceRequest): Observable<Response<any>> {
    return this.http.post<Response<any>>(`${this.baseUrl}/logout-device`, logoutRequest);
  }

  /**
   * Logout from all devices
   * @returns Observable with logout result
   */
  public logoutFromAllDevices(): Observable<Response<any>> {
    return this.http.post<Response<any>>(`${this.baseUrl}/logout-all`, {});
  }
}
