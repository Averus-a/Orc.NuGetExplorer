// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NuGetFeedVerificationService.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Catel;
    using Catel.Logging;
    using Catel.Scoping;
    using MethodTimer;
    using NuGet.Configuration;
    using NuGet.Protocol.Core.Types;
    using Scopes;

    internal class NuGetFeedVerificationService : INuGetFeedVerificationService
    {
        #region Fields
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly ISourceRepositoryProvider _packageRepositoryFactory;
        #endregion

        #region Constructors
        public NuGetFeedVerificationService(ISourceRepositoryProvider packageRepositoryFactory)
        {
            Argument.IsNotNull(() => packageRepositoryFactory);

            _packageRepositoryFactory = packageRepositoryFactory;
        }
        #endregion

        #region Methods
        [Time]
        public async Task<FeedVerificationResult> VerifyFeedAsync(string source, bool authenticateIfRequired, CancellationToken cancellationToken)
        {
            var result = FeedVerificationResult.Valid;

            Log.Debug("Verifying feed '{0}'", source);

            using (ScopeManager<AuthenticationScope>.GetScopeManager(source.GetSafeScopeName(), () => new AuthenticationScope(authenticateIfRequired)))
            {
                try
                {
                    var repository = _packageRepositoryFactory.CreateRepository(new PackageSource(source));
                    var searchResult = await repository.SearchAsync("", new SearchFilter(true), 1, cancellationToken);

                    foreach (var exception in searchResult.SearchExceptionBySource.Values)
                    {
                        switch (exception)
                        {
                            case WebException webException:
                                result = HandleWebException(webException, source);
                                break;

                            case UriFormatException uriFormatException:
                                Log.Debug(uriFormatException, "Failed to verify feed '{0}', a UriFormatException occurred", source);

                                result = FeedVerificationResult.Invalid;
                                break;

                            default:
                                Log.Debug(exception, "Failed to verify feed '{0}'", source);

                                result = FeedVerificationResult.Invalid;
                                break;
                        }

                        if (result != FeedVerificationResult.Valid)
                        {
                            break;
                        }
                    }
                }
                catch (WebException ex)
                {
                    result = HandleWebException(ex, source);
                }
                catch (UriFormatException ex)
                {
                    Log.Debug(ex, "Failed to verify feed '{0}', a UriFormatException occurred", source);

                    result = FeedVerificationResult.Invalid;
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Failed to verify feed '{0}'", source);

                    result = FeedVerificationResult.Invalid;
                }
            }

            Log.Debug("Verified feed '{0}', result is '{1}'", source, result);

            return result;
        }

        private static FeedVerificationResult HandleWebException(WebException exception, string source)
        {
            try
            {
                var httpWebResponse = (HttpWebResponse)exception.Response;
                if (ReferenceEquals(httpWebResponse, null))
                {
                    return FeedVerificationResult.Invalid;
                }

                if ((int)httpWebResponse.StatusCode == 403)
                {
                    return FeedVerificationResult.Valid;
                }

                if (exception.Status == WebExceptionStatus.ProtocolError)
                {
                    return httpWebResponse.StatusCode == HttpStatusCode.Unauthorized
                        ? FeedVerificationResult.AuthenticationRequired
                        : FeedVerificationResult.Invalid;
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Failed to verify feed '{0}'", source);
            }

            return FeedVerificationResult.Invalid;
        }
        #endregion
    }
}
