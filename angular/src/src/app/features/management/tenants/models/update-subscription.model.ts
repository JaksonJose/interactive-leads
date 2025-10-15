/**
 * Request model for updating a tenant's subscription information.
 */
export interface UpdateSubscriptionRequest {
  /** The unique identifier of the tenant whose subscription is being updated. */
  tenantId: string;

  /** The new expiration date for the tenant's subscription. */
  newExpirationDate: Date;
}
