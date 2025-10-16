using InteractiveLeads.Application.Responses.Messages;

namespace InteractiveLeads.Application.Responses
{

    /// <summary>
    /// Standard response wrapper for API operations without data payload.
    /// </summary>
    /// <remarks>
    /// Provides factory methods to create success and failure responses with optional messages.
    /// Used to maintain a consistent API response structure across all endpoints.
    /// </remarks>
    public class BaseResponse : IResponse
    {
        /// <summary>
        /// Gets or sets the list of messages associated with the response.
        /// </summary>
        public List<Message> Messages { get; set; } = [];

        /// <summary>
        /// Initializes a new instance of the ResponseWrapper class.
        /// </summary>
        public BaseResponse()
        {
        }

        public BaseResponse AddErrorMessage(string message, string code = "")
        {
            Messages.Add(new Message() { Text = message, Code = code, Type = MessageType.Error });
            return this;
        }

        public BaseResponse AddSuccessMessage(string message, string code = "")
        {
            Messages.Add(new Message() { Text = message, Code = code, Type = MessageType.Success });
            return this;
        }

        public BaseResponse AddInfoMessage(string message, string code = "")
        {
            Messages.Add(new Message() { Text = message, Code = code, Type = MessageType.Info });
            return this;
        }

        public BaseResponse AddWarningMessage(string message, string code = "")
        {
            Messages.Add(new Message() { Text = message, Code = code, Type = MessageType.Warning });
            return this;
        }
    }
}
