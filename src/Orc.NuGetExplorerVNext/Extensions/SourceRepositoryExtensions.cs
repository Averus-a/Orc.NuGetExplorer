// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SourceRepositoryExtensions.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using NuGet.Common;
    using NuGet.Packaging.Core;
    using NuGet.Protocol.Core.Types;
    using NuGet.Versioning;

    // from https://github.com/NuGet/NuGet.Client

    public static class SourceRepositoryExtensions
    {
        #region Methods
        public static Task<SearchResult<IPackageSearchMetadata>> SearchAsync(this SourceRepository sourceRepository, string searchText, SearchFilter searchFilter, int pageSize, CancellationToken cancellationToken)
        {
            var searchToken = new SearchCursor
            {
                SearchString = searchText,
                SearchFilter = searchFilter,
                StartIndex = 0
            };

            return sourceRepository.SearchAsync(searchToken, pageSize, cancellationToken);
        }

        public static async Task<SearchResult<IPackageSearchMetadata>> SearchAsync(
            this SourceRepository sourceRepository, SearchCursor searchCursor, int pageSize, CancellationToken cancellationToken)
        {
            var searchResource = await sourceRepository.GetResourceAsync<PackageSearchResource>(cancellationToken);

            var searchResults = await searchResource?.SearchAsync(
                searchCursor.SearchString,
                searchCursor.SearchFilter,
                searchCursor.StartIndex,
                pageSize + 1,
                NullLogger.Instance,
                cancellationToken);

            var items = searchResults?.ToArray() ?? new IPackageSearchMetadata[] { };

            var hasMoreItems = items.Length > pageSize;
            if (hasMoreItems)
            {
                items = items.Take(items.Length - 1).ToArray();
            }

            var result = new SearchResult<IPackageSearchMetadata>(items);

            var loadingStatus = hasMoreItems
                ? SearchStatus.Ready
                : items.Length == 0
                    ? SearchStatus.NothingFound
                    : SearchStatus.NoMoreFound;

            result.SearchStatusBySource = new Dictionary<string, SearchStatus>
            {
                {sourceRepository.PackageSource.Name, loadingStatus}
            };

            if (hasMoreItems)
            {
                result.Cursor = new SearchCursor
                {
                    SearchString = searchCursor.SearchString,
                    SearchFilter = searchCursor.SearchFilter,
                    StartIndex = searchCursor.StartIndex + items.Length
                };
            }

            return result;
        }

        public static async Task<IPackageSearchMetadata> GetPackageMetadataAsync(
            this SourceRepository sourceRepository, PackageIdentity identity, bool includePrerelease, CancellationToken cancellationToken)
        {
            var metadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>(cancellationToken);

            using (var sourceCacheContext = new SourceCacheContext())
            {
                // Update http source cache context MaxAge so that it can always go online to fetch
                // latest version of packages.
                sourceCacheContext.MaxAge = DateTimeOffset.UtcNow;

                var packages = await metadataResource?.GetMetadataAsync(
                    identity.Id,
                    includePrerelease: true,
                    includeUnlisted: false,
                    sourceCacheContext: sourceCacheContext,
                    log: NullLogger.Instance,
                    token: cancellationToken);

                if (packages?.FirstOrDefault() == null)
                {
                    return null;
                }

                var packageMetadata = packages
                                          .FirstOrDefault(p => p.Identity.Version == identity.Version)
                                      ?? PackageSearchMetadataBuilder.FromIdentity(identity).Build();

                return packageMetadata.WithVersions(ToVersionInfo(packages, includePrerelease));
            }
        }

        public static async Task<IPackageSearchMetadata> GetPackageMetadataFromLocalSourceAsync(
            this SourceRepository localRepository, PackageIdentity identity, CancellationToken cancellationToken)
        {
            var localResource = await localRepository.GetResourceAsync<PackageMetadataResource>(cancellationToken);

            using (var sourceCacheContext = new SourceCacheContext())
            {
                var localPackages = await localResource?.GetMetadataAsync(
                    identity.Id,
                    includePrerelease: true,
                    includeUnlisted: true,
                    sourceCacheContext: sourceCacheContext,
                    log: NullLogger.Instance,
                    token: cancellationToken);

                var packageMetadata = localPackages?.FirstOrDefault(p => p.Identity.Version == identity.Version);

                var versions = new[]
                {
                    new VersionInfo(identity.Version)
                };

                return packageMetadata?.WithVersions(versions);
            }
        }

        public static async Task<IPackageSearchMetadata> GetLatestPackageMetadataAsync(
            this SourceRepository sourceRepository, string packageId, bool includePrerelease, CancellationToken cancellationToken, VersionRange allowedVersions)
        {
            var metadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>(cancellationToken);

            using (var sourceCacheContext = new SourceCacheContext())
            {
                // Update http source cache context MaxAge so that it can always go online to fetch
                // latest version of packages.
                sourceCacheContext.MaxAge = DateTimeOffset.UtcNow;

                var packages = await metadataResource?.GetMetadataAsync(
                    packageId,
                    includePrerelease,
                    false,
                    sourceCacheContext,
                    NullLogger.Instance,
                    cancellationToken);

                // filter packages based on allowed versions
                var updatedPackages = packages.Where(p => allowedVersions.Satisfies(p.Identity.Version));

                var highest = updatedPackages
                    .OrderByDescending(e => e.Identity.Version, VersionComparer.VersionRelease)
                    .FirstOrDefault();

                return highest?.WithVersions(ToVersionInfo(packages, includePrerelease));
            }
        }

        public static async Task<IEnumerable<IPackageSearchMetadata>> GetPackageMetadataListAsync(
            this SourceRepository sourceRepository, string packageId, bool includePrerelease, bool includeUnlisted, CancellationToken cancellationToken)
        {
            var metadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>(cancellationToken);

            using (var sourceCacheContext = new SourceCacheContext())
            {
                // Update http source cache context MaxAge so that it can always go online to fetch
                // latest versions of the package.
                sourceCacheContext.MaxAge = DateTimeOffset.UtcNow;

                var packages = await metadataResource?.GetMetadataAsync(
                    packageId,
                    includePrerelease,
                    includeUnlisted,
                    sourceCacheContext,
                    NullLogger.Instance,
                    cancellationToken);

                return packages;
            }
        }

        private static IEnumerable<VersionInfo> ToVersionInfo(IEnumerable<IPackageSearchMetadata> packages, bool includePrerelease)
        {
            return packages?
                .Where(v => includePrerelease || !v.Identity.Version.IsPrerelease)
                .OrderByDescending(m => m.Identity.Version, VersionComparer.VersionRelease)
                .Select(m => new VersionInfo(m.Identity.Version, m.DownloadCount)
                {
                    PackageSearchMetadata = m
                });
        }
        #endregion
    }
}
