using InteractiveLeads.Application.Responses.Messages;

namespace InteractiveLeads.Application.Responses
{
    /// <summary>
    /// Interface for wrapping API responses with standard success/failure information and messages.
    /// </summary>
    /// <remarks>
    /// Provides a consistent response structure across all API endpoints.
    /// </remarks>
    public interface IResponse
    {
        /// <summary>
        /// Gets or sets the list of messages (errors or informational) associated with the response.
        /// </summary>
        //List<string> Messages { get; set; }

        List<Message> Messages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccessful { get; set; }
    }

    /// <summary>
    /// Generic interface for wrapping API responses that include data.
    /// </summary>
    /// <typeparam name="T">The type of data being returned in the response.</typeparam>
    /// <remarks>
    /// Extends IResponseWrapper to include strongly-typed data payload.
    /// </remarks>
    public interface IResponse<T> : IResponse where T : class
    {
        /// <summary>
        /// Gets the single data payload of the response.
        /// </summary>
        T? Data { get; }

        /// <summary>
        /// Gets the list data payload of the response.
        /// </summary>
        List<T> Items { get; }
    }
}
