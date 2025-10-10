using System.Net;

namespace InteractiveLeads.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when authentication fails or is required but not provided.
    /// </summary>
    /// <remarks>
    /// This exception maps to HTTP 401 Unauthorized status code and indicates that
    /// the user must authenticate before accessing the resource. This is different from
    /// ForbiddenException (403) which indicates the user is authenticated but lacks permissions.
    /// </remarks>
    public class UnauthorizedException : Exception
    {
        /// <summary>
        /// Gets or sets the list of error messages describing the authentication failure.
        /// </summary>
        public List<string> ErrorMessages { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status code associated with this exception.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the UnauthorizedException class.
        /// </summary>
        /// <param name="errorMessages">The list of error messages describing the authentication failure.</param>
        /// <param name="statusCode">The HTTP status code (defaults to Unauthorized).</param>
        public UnauthorizedException(List<string> errorMessages = default!, HttpStatusCode statusCode = HttpStatusCode.Unauthorized)
        {
            StatusCode = statusCode;
            ErrorMessages = errorMessages;
        }
    }
}
