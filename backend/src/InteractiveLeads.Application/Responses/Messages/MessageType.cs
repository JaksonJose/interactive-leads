namespace InteractiveLeads.Application.Responses.Messages
{
    /// <summary>
    /// Enumeration of message types for categorizing different types of feedback.
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Error message - indicates operation failure.
        /// </summary>
        Error,

        /// <summary>
        /// Success message - indicates successful operation.
        /// </summary>
        Success,

        /// <summary>
        /// Informational message - provides additional information.
        /// </summary>
        Info,

        /// <summary>
        /// Warning message - indicates a situation that requires attention.
        /// </summary>
        Warning
    }
}
