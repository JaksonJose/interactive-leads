import { TokenResponse } from './token-response';

/**
 * Base interface for all API responses
 */
export interface BaseResponseWrapper {
  messages: string[];
  isSuccessful: boolean;
}

/**
 * Generic interface for responses with data payload
 */
export interface ResponseWrapper<T> extends BaseResponseWrapper {
  data?: T;
}

/**
 * Type aliases for specific response types (more efficient than empty interfaces)
 */
export type TokenResponseWrapper = ResponseWrapper<TokenResponse>;
export type LoginResponseWrapper = ResponseWrapper<TokenResponse>;
export type RefreshTokenResponseWrapper = ResponseWrapper<TokenResponse>;

/**
 * Generic response wrapper for unknown data types
 */
export type GenericResponseWrapper = ResponseWrapper<unknown>;

/**
 * Union type for all possible response wrappers
 */
export type ApiResponse = 
  | TokenResponseWrapper 
  | LoginResponseWrapper 
  | RefreshTokenResponseWrapper 
  | GenericResponseWrapper;
