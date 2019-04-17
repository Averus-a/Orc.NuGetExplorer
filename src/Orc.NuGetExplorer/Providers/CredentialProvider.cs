// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CredentialProvider.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Catel;
    using NuGet.Configuration;
    using NuGet.Credentials;

    internal class CredentialProvider : ICredentialProvider
    {
        #region Fields
        private readonly IAuthenticationProvider _authenticationProvider;
        #endregion

        #region Constructors
        public CredentialProvider(IAuthenticationProvider authenticationProvider)
        {
            Argument.IsNotNull(() => authenticationProvider);

            _authenticationProvider = authenticationProvider;
        }
        #endregion

        #region Properties
        public string Id { get; }
        #endregion

        #region Methods
        public async Task<CredentialResponse> GetAsync(Uri uri, IWebProxy proxy, CredentialRequestType type, string message, bool isRetry, bool nonInteractive,
            CancellationToken cancellationToken)
        {
            var credentials = await _authenticationProvider.GetCredentialsAsync(uri, isRetry, cancellationToken);

            if (credentials is null)
            {
                return new CredentialResponse(CredentialStatus.UserCanceled);
            }

            if (string.IsNullOrWhiteSpace(credentials.UserName) && string.IsNullOrWhiteSpace(credentials.Password))
            {
                return new CredentialResponse(CredentialStatus.ProviderNotApplicable);
            }

            var networkCredential = new NetworkCredential(credentials.UserName, credentials.Password);

            return new CredentialResponse(networkCredential);
        }
        #endregion
    }
}
