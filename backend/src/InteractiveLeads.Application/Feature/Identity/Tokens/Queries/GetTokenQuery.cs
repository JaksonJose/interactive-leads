﻿using InteractiveLeads.Application.Wrappers;
using MediatR;

namespace InteractiveLeads.Application.Feature.Identity.Tokens.Queries
{
    /// <summary>
    /// Query for authenticating a user and obtaining JWT tokens.
    /// </summary>
    /// <remarks>
    /// This query implements the CQRS pattern for user login operations.
    /// </remarks>
    public class GetTokenQuery : IRequest<IResponseWrapper>
    {
        /// <summary>
        /// Gets or sets the token request containing login credentials.
        /// </summary>
        public TokenRequest TokenRequest { get; set; } = new();
    }

    /// <summary>
    /// Handler for processing GetTokenQuery requests.
    /// </summary>
    /// <remarks>
    /// Authenticates the user via ITokenService and returns wrapped token response.
    /// </remarks>
    public class GetTokenQueryHandler : IRequestHandler<GetTokenQuery, IResponseWrapper>
    {
        private readonly ITokenService _tokenService;

        /// <summary>
        /// Initializes a new instance of the GetTokenQueryHandler class.
        /// </summary>
        /// <param name="tokenService">The token service for authentication operations.</param>
        public GetTokenQueryHandler(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        /// <summary>
        /// Handles the GetTokenQuery request and authenticates the user.
        /// </summary>
        /// <param name="request">The query containing authentication credentials.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>A wrapped response containing the JWT tokens if authentication succeeds.</returns>
        public async Task<IResponseWrapper> Handle(GetTokenQuery request, CancellationToken cancellationToken)
        {
            var token = await _tokenService.LoginAsync(request.TokenRequest);

            return await ResponseWrapper<TokenResponse>.SuccessAsync(data: token);
        }
    }
}
