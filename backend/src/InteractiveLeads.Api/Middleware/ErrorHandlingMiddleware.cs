using InteractiveLeads.Application.Exceptions;
using InteractiveLeads.Application.Wrappers;
using System.Net;
using System.Text.Json;

namespace InteractiveLeads.Api.Middleware
{
    /// <summary>
    /// Middleware for global exception handling and error response formatting.
    /// </summary>
    /// <remarks>
    /// Catches all exceptions thrown during request processing and converts them
    /// into appropriate HTTP responses with consistent error formatting.
    /// </remarks>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the ErrorHandlingMiddleware.
        /// </summary>
        /// <param name="next">The next middleware in the request pipeline.</param>
        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Processes the HTTP request and handles any exceptions that occur.
        /// </summary>
        /// <param name="context">The HTTP context for the current request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// Maps custom exceptions to appropriate HTTP status codes and formats error responses.
        /// Unhandled exceptions are mapped to 500 Internal Server Error.
        /// </remarks>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex) 
            {
                var response = context.Response;
                response.ContentType = "application/json";

                var responseWrapper = ResponseWrapper.Fail();

                switch (ex)
                {
                    case ConflictException ce:
                        response.StatusCode = (int)ce.StatusCode;
                        responseWrapper.Messages = ce.ErrorMessages;
                        break;
                    case NotFoundException nfe:
                        response.StatusCode = (int)nfe.StatusCode;
                        responseWrapper.Messages = nfe.ErrorMessages;
                        break;
                    case ForbiddenException fe:
                        response.StatusCode = (int)fe.StatusCode;
                        responseWrapper.Messages = fe.ErrorMessages;
                        break;
                    case IdentityException ie:
                        response.StatusCode = (int)ie.StatusCode;
                        responseWrapper.Messages = ie.ErrorMessages;
                        break;
                    case UnauthorizedException ue:
                        response.StatusCode = (int)ue.StatusCode;
                        responseWrapper.Messages = ue.ErrorMessages;
                        break;
                    default:
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        responseWrapper.Messages = ["Something went wrong"];
                        break;
                }

                var result = JsonSerializer.Serialize(responseWrapper);

                await response.WriteAsync(result);
            }
        }
    }
}
