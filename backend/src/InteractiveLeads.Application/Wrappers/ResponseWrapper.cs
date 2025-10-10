namespace InteractiveLeads.Application.Wrappers
{
    /// <summary>
    /// Standard response wrapper for API operations without data payload.
    /// </summary>
    /// <remarks>
    /// Provides factory methods to create success and failure responses with optional messages.
    /// Used to maintain a consistent API response structure across all endpoints.
    /// </remarks>
    public class ResponseWrapper : IResponseWrapper
    {
        /// <summary>
        /// Gets or sets the list of messages associated with the response.
        /// </summary>
        public List<string> Messages { get; set; } = [];

        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Initializes a new instance of the ResponseWrapper class.
        /// </summary>
        public ResponseWrapper()
        {            
        }

        /// <summary>
        /// Creates a failure response without any messages.
        /// </summary>
        /// <returns>A response wrapper indicating failure.</returns>
        public static IResponseWrapper Fail()
        {
            return new ResponseWrapper
            {
                IsSuccessful = false,
            };
        }

        /// <summary>
        /// Creates a failure response with a single error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <returns>A response wrapper indicating failure with the provided message.</returns>
        public static IResponseWrapper Fail(string message)
        {
            return new ResponseWrapper
            {
                IsSuccessful = false,
                Messages = [message]
            };
        }

        /// <summary>
        /// Creates a failure response with multiple error messages.
        /// </summary>
        /// <param name="messages">The list of error messages.</param>
        /// <returns>A response wrapper indicating failure with the provided messages.</returns>
        public static IResponseWrapper Fail(List<string> messages)
        {
            return new ResponseWrapper { IsSuccessful = false, Messages = messages };
        }

        /// <summary>
        /// Creates a failure response asynchronously without any messages.
        /// </summary>
        /// <returns>A task containing a response wrapper indicating failure.</returns>
        public static Task<IResponseWrapper> FailAsync()
        {
            return Task.FromResult(Fail());
        }

        /// <summary>
        /// Creates a failure response asynchronously with a single error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <returns>A task containing a response wrapper indicating failure with the provided message.</returns>
        public static Task<IResponseWrapper> FailAsync(string message)
        {
            return Task.FromResult(Fail(message));
        }

        /// <summary>
        /// Creates a failure response asynchronously with multiple error messages.
        /// </summary>
        /// <param name="messages">The list of error messages.</param>
        /// <returns>A task containing a response wrapper indicating failure with the provided messages.</returns>
        public static Task<IResponseWrapper> FailAsync(List<string> messages)
        {
            return Task.FromResult(Fail(messages));
        }

        /// <summary>
        /// Creates a success response without any messages.
        /// </summary>
        /// <returns>A response wrapper indicating success.</returns>
        public static IResponseWrapper Success()
        {
            return new ResponseWrapper { IsSuccessful = true };
        }

        /// <summary>
        /// Creates a success response with a single message.
        /// </summary>
        /// <param name="message">The success message.</param>
        /// <returns>A response wrapper indicating success with the provided message.</returns>
        public static IResponseWrapper Success(string message)
        {
            return new ResponseWrapper { IsSuccessful = true, Messages = [message] };
        }

        /// <summary>
        /// Creates a success response with multiple messages.
        /// </summary>
        /// <param name="messages">The list of success messages.</param>
        /// <returns>A response wrapper indicating success with the provided messages.</returns>
        public static IResponseWrapper Success(List<string> messages)
        {
            return new ResponseWrapper { IsSuccessful = true, Messages = messages };
        }

        /// <summary>
        /// Creates a success response asynchronously without any messages.
        /// </summary>
        /// <returns>A task containing a response wrapper indicating success.</returns>
        public static Task<IResponseWrapper> SuccessAsync()
        {
            return Task.FromResult(Success());
        }

        /// <summary>
        /// Creates a success response asynchronously with a single message.
        /// </summary>
        /// <param name="message">The success message.</param>
        /// <returns>A task containing a response wrapper indicating success with the provided message.</returns>
        public static Task<IResponseWrapper> SuccessAsync(string message)
        {
            return Task.FromResult(Success(message));
        }

        /// <summary>
        /// Creates a success response asynchronously with multiple messages.
        /// </summary>
        /// <param name="messages">The list of success messages.</param>
        /// <returns>A task containing a response wrapper indicating success with the provided messages.</returns>
        public static Task<IResponseWrapper> SuccessAsync(List<string> messages)
        {
            return Task.FromResult(Success(messages));
        }
    }

    /// <summary>
    /// Generic response wrapper for API operations that include a data payload.
    /// </summary>
    /// <typeparam name="T">The type of data being returned.</typeparam>
    /// <remarks>
    /// Extends ResponseWrapper to include strongly-typed data in addition to success/failure information.
    /// Provides factory methods for creating responses with data payloads.
    /// </remarks>
    public class ResponseWrapper<T> : ResponseWrapper, IResponseWrapper<T> where T : class
    {
        /// <summary>
        /// Gets or sets the data payload of the response.
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Creates a failure response without any messages.
        /// </summary>
        /// <returns>A generic response wrapper indicating failure.</returns>
        public static new ResponseWrapper<T> Fail()
        {
            return new ResponseWrapper<T>
            {
                IsSuccessful = false,
            };
        }

        /// <summary>
        /// Creates a failure response with a single error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <returns>A generic response wrapper indicating failure with the provided message.</returns>
        public static new ResponseWrapper<T> Fail(string message)
        {
            return new ResponseWrapper<T>
            {
                IsSuccessful = false,
                Messages = [message]
            };
        }

        /// <summary>
        /// Creates a failure response with multiple error messages.
        /// </summary>
        /// <param name="messages">The list of error messages.</param>
        /// <returns>A generic response wrapper indicating failure with the provided messages.</returns>
        public static new ResponseWrapper<T> Fail(List<string> messages)
        {
            return new ResponseWrapper<T> { IsSuccessful = false, Messages = messages };
        }

        /// <summary>
        /// Creates a failure response asynchronously without any messages.
        /// </summary>
        /// <returns>A task containing a generic response wrapper indicating failure.</returns>
        public static new Task<ResponseWrapper<T>> FailAsync()
        {
            return Task.FromResult(Fail());
        }

        /// <summary>
        /// Creates a failure response asynchronously with a single error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <returns>A task containing a generic response wrapper indicating failure with the provided message.</returns>
        public static new Task<ResponseWrapper<T>> FailAsync(string message)
        {
            return Task.FromResult(Fail(message));
        }

        /// <summary>
        /// Creates a failure response asynchronously with multiple error messages.
        /// </summary>
        /// <param name="messages">The list of error messages.</param>
        /// <returns>A task containing a generic response wrapper indicating failure with the provided messages.</returns>
        public static new Task<ResponseWrapper<T>> FailAsync(List<string> messages)
        {
            return Task.FromResult(Fail(messages));
        }

        /// <summary>
        /// Creates a success response without any messages or data.
        /// </summary>
        /// <returns>A generic response wrapper indicating success.</returns>
        public static new ResponseWrapper<T> Success()
        {
            return new ResponseWrapper<T> { IsSuccessful = true };
        }

        /// <summary>
        /// Creates a success response with a single message.
        /// </summary>
        /// <param name="message">The success message.</param>
        /// <returns>A generic response wrapper indicating success with the provided message.</returns>
        public static new ResponseWrapper<T> Success(string message)
        {
            return new ResponseWrapper<T> { IsSuccessful = true, Messages = [message] };
        }

        /// <summary>
        /// Creates a success response with multiple messages.
        /// </summary>
        /// <param name="messages">The list of success messages.</param>
        /// <returns>A generic response wrapper indicating success with the provided messages.</returns>
        public static new ResponseWrapper<T> Success(List<string> messages)
        {
            return new ResponseWrapper<T> { IsSuccessful = true, Messages = messages };
        }

        /// <summary>
        /// Creates a success response asynchronously without any messages.
        /// </summary>
        /// <returns>A task containing a generic response wrapper indicating success.</returns>
        public static new Task<ResponseWrapper<T>> SuccessAsync()
        {
            return Task.FromResult(Success());
        }

        /// <summary>
        /// Creates a success response asynchronously with a single message.
        /// </summary>
        /// <param name="message">The success message.</param>
        /// <returns>A task containing a generic response wrapper indicating success with the provided message.</returns>
        public static new Task<ResponseWrapper<T>> SuccessAsync(string message)
        {
            return Task.FromResult(Success(message));
        }

        /// <summary>
        /// Creates a success response asynchronously with multiple messages.
        /// </summary>
        /// <param name="messages">The list of success messages.</param>
        /// <returns>A task containing a generic response wrapper indicating success with the provided messages.</returns>
        public static new Task<ResponseWrapper<T>> SuccessAsync(List<string> messages)
        {
            return Task.FromResult(Success(messages));
        }

        /// <summary>
        /// Creates a success response with data.
        /// </summary>
        /// <param name="data">The data payload.</param>
        /// <returns>A generic response wrapper indicating success with the provided data.</returns>
        public static ResponseWrapper<T> Success(T data)
        {
            return new ResponseWrapper<T> { IsSuccessful = true, Data = data };
        }

        /// <summary>
        /// Creates a success response with data and a single message.
        /// </summary>
        /// <param name="data">The data payload.</param>
        /// <param name="message">The success message.</param>
        /// <returns>A generic response wrapper indicating success with the provided data and message.</returns>
        public static ResponseWrapper<T> Success(T data, string message)
        {
            return new ResponseWrapper<T> { Data = data, IsSuccessful = true, Messages = [message] };
        }

        /// <summary>
        /// Creates a success response with data and multiple messages.
        /// </summary>
        /// <param name="data">The data payload.</param>
        /// <param name="messages">The list of success messages.</param>
        /// <returns>A generic response wrapper indicating success with the provided data and messages.</returns>
        public static ResponseWrapper<T> Success(T data, List<string> messages)
        {
            return new ResponseWrapper<T> { Data = data, IsSuccessful = true, Messages = messages };
        }

        /// <summary>
        /// Creates a success response asynchronously with data.
        /// </summary>
        /// <param name="data">The data payload.</param>
        /// <returns>A task containing a generic response wrapper indicating success with the provided data.</returns>
        public static Task<ResponseWrapper<T>> SuccessAsync(T data)
        {
            return Task.FromResult(Success(data));
        }

        /// <summary>
        /// Creates a success response asynchronously with data and a single message.
        /// </summary>
        /// <param name="data">The data payload.</param>
        /// <param name="message">The success message.</param>
        /// <returns>A task containing a generic response wrapper indicating success with the provided data and message.</returns>
        public static Task<ResponseWrapper<T>> SuccessAsync(T data, string message)
        {
            return Task.FromResult(Success(data, message));
        }

        /// <summary>
        /// Creates a success response asynchronously with data and multiple messages.
        /// </summary>
        /// <param name="data">The data payload.</param>
        /// <param name="messages">The list of success messages.</param>
        /// <returns>A task containing a generic response wrapper indicating success with the provided data and messages.</returns>
        public static Task<ResponseWrapper<T>> SuccessAsync(T data, List<string> messages)
        {
            return Task.FromResult(Success(data, messages));
        }
    }
}
