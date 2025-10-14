namespace InteractiveLeads.Application.Responses
{
    /// <summary>
    /// Generic response wrapper for API operations that include a data payload.
    /// </summary>
    /// <typeparam name="T">The type of data being returned.</typeparam>
    /// <remarks>
    /// Extends ResponseWrapper to include strongly-typed data in addition to success/failure information.
    /// </remarks>
    public class Response<T> : BaseResponse, IResponse<T> where T : class
    {
        /// <summary>
        /// Gets or sets the data payload of the response.
        /// </summary>
        public T? Data { get; set; }

        public List<T> Items { get; set; } = [];

        public Response() : base()
        {
        }

        public Response(T item) : base()
        {
            this.Data = item;
        }

        public Response(List<T> items) : base()
        {
            this.Items = items;
        }
    }

    public class Response : BaseResponse, IResponse
    {
        public Response() : base()
        {
        }
    }
}
