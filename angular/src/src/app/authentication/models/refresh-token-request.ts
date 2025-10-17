/**
 * Request model for refreshing an expired JWT access token.
 */
export class RefreshTokenRequest {
  /**
   * The current (potentially expired) JWT access token.
   */
  public currentJwt: string = '';

  /**
   * The current refresh token used to obtain a new JWT.
   */
  public currentRefreshToken: string = '';

  /**
   * The expiration date of the refresh token.
   */
  public refreshTokenExpiryDate: Date = new Date();
}
