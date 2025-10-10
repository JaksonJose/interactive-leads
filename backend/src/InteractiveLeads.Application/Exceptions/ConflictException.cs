using System.Net;

namespace InteractiveLeads.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when a conflict occurs during an operation.
    /// </summary>
    /// <remarks>
    /// This exception is typically used for scenarios such as duplicate entries,
    /// concurrent modifications, or resource conflicts. It maps to HTTP 409 Conflict status code.
    /// </remarks>
    public class ConflictException : Exception
    {
        /// <summary>
        /// Gets or sets the list of error messages describing the conflict.
        /// </summary>
        public List<string> ErrorMessages { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status code associated with this exception.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the ConflictException class.
        /// </summary>
        /// <param name="errorMessages">The list of error messages describing the conflict.</param>
        /// <param name="statusCode">The HTTP status code (defaults to Forbidden).</param>
        public ConflictException(List<string> errorMessages = default!, HttpStatusCode statusCode = HttpStatusCode.Forbidden)
        {
            ErrorMessages = errorMessages;
            StatusCode = statusCode;
        }
    }
}
