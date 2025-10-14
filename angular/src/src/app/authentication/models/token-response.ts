/**
 * Response model containing JWT authentication tokens
 */
export interface TokenResponse {
  jwt: string;
  refreshToken: string;
  refreshTokenExpirationDate: Date;
}
