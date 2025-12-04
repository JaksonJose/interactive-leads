/**
 * Represents a user in a tenant.
 */
export interface User {
  /** The unique identifier for the user. */
  id: string;

  /** The user's first name. */
  firstName: string;

  /** The user's last name. */
  lastName: string;

  /** The user's email address. */
  email: string;

  /** The user's username. */
  userName: string;

  /** The user's phone number. */
  phoneNumber?: string;

  /** Indicates whether the user account is active. */
  isActive: boolean;
}

/**
 * Request model for creating a new user in a tenant.
 */
export interface CreateUserRequest {
  /** The user's first name. */
  firstName: string;

  /** The user's last name. */
  lastName: string;

  /** The user's email address. */
  email: string;

  /** The user's password. */
  password: string;

  /** Password confirmation. */
  confirmPassword: string;

  /** The user's phone number. */
  phoneNumber?: string;

  /** Indicates whether the user account should be active. */
  isActive: boolean;
}

/**
 * Request model for updating an existing user in a tenant.
 */
export interface UpdateUserRequest {
  /** The unique identifier for the user. */
  id: string;

  /** The user's first name. */
  firstName: string;

  /** The user's last name. */
  lastName: string;

  /** The user's phone number. */
  phoneNumber?: string;
}

