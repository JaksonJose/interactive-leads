using System.Net;

namespace InteractiveLeads.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when an identity-related operation fails.
    /// </summary>
    /// <remarks>
    /// This exception is used for identity system errors such as user creation failures,
    /// role assignment issues, or other identity management problems.
    /// Maps to HTTP 403 Forbidden status code by default.
    /// </remarks>
    public class IdentityException : Exception
    {
        /// <summary>
        /// Gets or sets the list of error messages describing the identity error.
        /// </summary>
        public List<string> ErrorMessages { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status code associated with this exception.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the IdentityException class.
        /// </summary>
        /// <param name="errorMessages">The list of error messages describing the identity error.</param>
        /// <param name="statusCode">The HTTP status code (defaults to Forbidden).</param>
        public IdentityException(List<string> errorMessages = default!, HttpStatusCode statusCode = HttpStatusCode.Forbidden)
        {
            ErrorMessages = errorMessages;
            StatusCode = statusCode;
        }
    }
}
