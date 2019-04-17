// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SourceRepositoryExtensions.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using NuGet.Common;
    using NuGet.Protocol.Core.Types;

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
        #endregion
    }
}
