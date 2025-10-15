/**
 * Request model for creating a new tenant in the multi-tenant system.
 */
export interface CreateTenantRequest {
  /** The unique identifier for the tenant. */
  identifier: string;

  /** The display name of the tenant organization. */
  name: string;

  /** The email address of the tenant's primary administrator. */
  email: string;

  /** The first name of the tenant's primary administrator. */
  firstName: string;

  /** The last name of the tenant's primary administrator. */
  lastName: string;

  /** The expiration date of the tenant's subscription. */
  expirationDate: Date;

  /** Indicates whether the tenant is active. */
  isActive: boolean;

  /** The database connection string for the tenant's isolated data (optional). */
  connectionString?: string;
}
