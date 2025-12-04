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

  /** The user's roles. */
  roles?: string[];
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

  /** The roles to assign to the user. */
  roles?: string[];
}

/**
 * Represents a role in the system.
 */
export interface Role {
  /** The unique identifier for the role. */
  id: string;

  /** The role name. */
  name: string;

  /** The role description. */
  description?: string;
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

  /** The roles to assign to the user. */
  roles?: string[];
}

