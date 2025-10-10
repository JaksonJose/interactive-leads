using System.Net;

namespace InteractiveLeads.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when a user attempts to access a resource they don't have permission for.
    /// </summary>
    /// <remarks>
    /// This exception maps to HTTP 403 Forbidden status code and indicates that
    /// the user is authenticated but lacks the necessary permissions for the requested operation.
    /// </remarks>
    public class ForbiddenException : Exception
    {
        /// <summary>
        /// Gets or sets the list of error messages describing why access was forbidden.
        /// </summary>
        public List<string> ErrorMessages { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status code associated with this exception.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the ForbiddenException class.
        /// </summary>
        /// <param name="errorMessages">The list of error messages describing the forbidden access.</param>
        /// <param name="statusCode">The HTTP status code (defaults to Forbidden).</param>
        public ForbiddenException(List<string> errorMessages = default!, HttpStatusCode statusCode = HttpStatusCode.Forbidden) 
        {
            ErrorMessages = errorMessages;
            StatusCode = statusCode;
        }
    }
}
