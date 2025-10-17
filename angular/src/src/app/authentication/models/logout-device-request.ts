/**
 * Request model for logging out from a specific device.
 */
export class LogoutDeviceRequest {
  /**
   * The refresh token to revoke for device-specific logout.
   */
  public refreshToken: string = '';
}
