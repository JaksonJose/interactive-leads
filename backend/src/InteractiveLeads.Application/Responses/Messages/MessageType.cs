namespace InteractiveLeads.Application.Responses.Messages
{
    /// <summary>
    /// Enumeração dos tipos de mensagem para categorizar diferentes tipos de feedback.
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Mensagem de erro - indica falha na operação.
        /// </summary>
        Error,

        /// <summary>
        /// Mensagem de sucesso - indica operação bem-sucedida.
        /// </summary>
        Success,

        /// <summary>
        /// Mensagem informativa - fornece informações adicionais.
        /// </summary>
        Info,

        /// <summary>
        /// Mensagem de aviso - indica situação que requer atenção.
        /// </summary>
        Warning
    }
}
