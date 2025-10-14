namespace InteractiveLeads.Application.Responses.Messages
{
    /// <summary>
    /// Representa uma mensagem estruturada com texto, código e tipo.
    /// </summary>
    public sealed class Message
    {
        /// <summary>
        /// O texto da mensagem.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// O código da mensagem para internacionalização.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// O tipo da mensagem (Error, Success, Info, Warning).
        /// </summary>
        public MessageType Type { get; set; } = MessageType.Info;
    }
}
