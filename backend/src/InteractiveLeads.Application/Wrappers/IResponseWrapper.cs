namespace InteractiveLeads.Application.Wrappers
{
    /// <summary>
    /// Interface for wrapping API responses with standard success/failure information and messages.
    /// </summary>
    /// <remarks>
    /// Provides a consistent response structure across all API endpoints.
    /// </remarks>
    public interface IResponseWrapper
    {
        /// <summary>
        /// Gets or sets the list of messages (errors or informational) associated with the response.
        /// </summary>
        List<string> Messages { get; set; }

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
    public interface IResponseWrapper<out T> : IResponseWrapper where T : class
    {
        /// <summary>
        /// Gets the data payload of the response.
        /// </summary>
        T? Data { get; }
    }
}
