// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SourceRepositoryExtensions.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
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
    using NuGet.Protocol.Core.Types;

    internal static class SourceRepositoryExtensions
    {
        #region Methods
        public static Task<SearchResult<IPackage>> SearchAsync(this SourceRepository sourceRepository, string searchText, SearchFilter searchFilter, int pageSize, CancellationToken cancellationToken)
        {
            var searchToken = new SearchCursor
            {
                SearchString = searchText,
                SearchFilter = searchFilter,
                StartIndex = 0
            };

            return sourceRepository.SearchAsync(searchToken, pageSize, cancellationToken);
        }

        public static async Task<SearchResult<IPackage>> SearchAsync(
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

            var items = searchResults?.Select(x => new Package(x, sourceRepository)).ToArray() ?? new IPackage[] { };

            var hasMoreItems = items.Length > pageSize;
            if (hasMoreItems)
            {
                items = items.Take(items.Length - 1).ToArray();
            }

            var result = new SearchResult<IPackage>(items);

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

        public static async Task<IEnumerable<IPackage>> GetPackageDataListAsync(
            this SourceRepository sourceRepository, string packageId, bool includePrerelease, bool includeUnlisted, CancellationToken cancellationToken)
        {
            var metadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>(cancellationToken);

            using (var sourceCacheContext = new SourceCacheContext())
            {
                // Update http source cache context MaxAge so that it can always go online to fetch
                // latest versions of the searchMetadata.
                sourceCacheContext.MaxAge = DateTimeOffset.UtcNow;

                var packages = await metadataResource?.GetMetadataAsync(
                    packageId,
                    includePrerelease,
                    includeUnlisted,
                    sourceCacheContext,
                    NullLogger.Instance,
                    cancellationToken);

                return packages.Select(x => new Package(x, sourceRepository));
            }
        }
        #endregion
    }
}
