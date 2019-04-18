// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InstalledPackageFeed.cs" company="WildGums">
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
    using Catel;
    using NuGet.Packaging.Core;
    using NuGet.Protocol.Core.Types;
    using NuGet.Common;

    public sealed class InstalledPackageFeed : SingleSourcePackageFeedBase
    {
        private readonly IEnumerable<PackageCollectionItem> _installedPackages;
        private readonly IPackageMetadataProvider _metadataProvider;
        private readonly ILogger _logger;

        public InstalledPackageFeed(
            IEnumerable<PackageCollectionItem> installedPackages,
            IPackageMetadataProvider metadataProvider,
            ILogger logger)
        {
            Argument.IsNotNull(() => installedPackages);
            Argument.IsNotNull(() => metadataProvider);
            Argument.IsNotNull(() => logger);

            _installedPackages = installedPackages;
            _metadataProvider = metadataProvider;
            _logger = logger;

            PageSize = 25;
        }

        public override async Task<SearchResult<IPackageSearchMetadata>> ContinueSearchAsync(SearchCursor searchCursor, CancellationToken cancellationToken)
        {
            if (searchCursor == null)
            {
                throw new InvalidOperationException("Invalid token");
            }

            var packages = _installedPackages
                .GetLatest()
                .Where(p => p.Id.IndexOf(searchCursor.SearchString, StringComparison.OrdinalIgnoreCase) != -1)
                .OrderBy(p => p.Id)
                .Skip(searchCursor.StartIndex)
                .Take(PageSize + 1)
                .ToArray();

            var hasMoreItems = packages.Length > PageSize;
            if (hasMoreItems)
            {
                packages = packages.Take(packages.Length - 1).ToArray();
            }

            var items = await TaskCombinators.ThrottledAsync(
                packages,
                (p, t) => GetPackageMetadataAsync(p, searchCursor.SearchFilter.IncludePrerelease, t),
                cancellationToken);

            //  The packages were originally sorted which is important because we Skip and Take based on that sort
            //  however the asynchronous execution has randomly reordered the set. So we need to resort. 
            var result = SearchResult.FromItems(items.OrderBy(p => p.Identity.Id).ToArray());

            var loadingStatus = hasMoreItems
                ? SearchStatus.Ready
                : packages.Length == 0
                ? SearchStatus.NothingFound
                : SearchStatus.NoMoreFound;
            result.SearchStatusBySource = new Dictionary<string, SearchStatus>
            {
                { "Installed", loadingStatus }
            };

            if (hasMoreItems)
            {
                result.Cursor = new SearchCursor()
                {
                    SearchString = searchCursor.SearchString,
                    SearchFilter = searchCursor.SearchFilter,
                    StartIndex = searchCursor.StartIndex + packages.Length
                };
            }

            return result;
        }

        public async Task<IPackageSearchMetadata> GetPackageMetadataAsync(PackageIdentity identity, bool includePrerelease, CancellationToken cancellationToken)
        {
            // first we try and load the metadata from a local package
            var packageMetadata = await _metadataProvider.GetLocalPackageMetadataAsync(identity, includePrerelease, cancellationToken);
            if (packageMetadata == null)
            {
                // and failing that we go to the network
                packageMetadata = await _metadataProvider.GetPackageMetadataAsync(identity, includePrerelease, cancellationToken);
            }
            return packageMetadata;
        }
    }
}
