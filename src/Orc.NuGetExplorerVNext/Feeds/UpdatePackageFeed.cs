// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdatePackageFeed.cs" company="WildGums">
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
    using NuGet.Common;
    using NuGet.ProjectManagement;
    using NuGet.Protocol.Core.Types;
    using NuGet.Versioning;

    public sealed class UpdatePackageFeed : SingleSourcePackageFeedBase
    {
        #region Fields
        private readonly PackageSearchMetadataCache _cachedUpdates;
        private readonly IEnumerable<PackageCollectionItem> _installedPackages;
        private readonly ILogger _logger;
        private readonly IPackageMetadataProvider _metadataProvider;
        private readonly NuGetProject[] _projects;
        #endregion

        #region Constructors
        public UpdatePackageFeed(
            IEnumerable<PackageCollectionItem> installedPackages,
            IPackageMetadataProvider metadataProvider,
            NuGetProject[] projects,
            PackageSearchMetadataCache optionalCachedUpdates,
            ILogger logger)
        {
            Argument.IsNotNull(() => installedPackages);
            Argument.IsNotNull(() => metadataProvider);
            Argument.IsNotNull(() => projects);
            Argument.IsNotNull(() => logger);

            _installedPackages = installedPackages;
            _metadataProvider = metadataProvider;
            _projects = projects;
            _cachedUpdates = optionalCachedUpdates;
            _logger = logger;
        }
        #endregion

        #region Methods
        public override async Task<SearchResult<IPackageSearchMetadata>> ContinueSearchAsync(SearchCursor searchCursor, CancellationToken cancellationToken)
        {
            if (searchCursor == null)
            {
                throw new InvalidOperationException("Invalid token");
            }

            var packagesWithUpdates = _cachedUpdates?.IncludePrerelease == searchCursor.SearchFilter.IncludePrerelease
                ? GetPackagesFromCache(searchCursor.SearchString)
                : await GetPackagesWithUpdatesAsync(searchCursor.SearchString, searchCursor.SearchFilter, cancellationToken);

            var items = packagesWithUpdates
                .Skip(searchCursor.StartIndex)
                .ToArray();

            var result = SearchResult.FromItems(items);

            var loadingStatus = items.Length == 0
                ? SearchStatus.NothingFound
                : SearchStatus.NoMoreFound;
            result.SearchStatusBySource = new Dictionary<string, SearchStatus>
            {
                ["Update"] = loadingStatus
            };

            return result;
        }

        private IEnumerable<IPackageSearchMetadata> GetPackagesFromCache(string searchText)
        {
            return _cachedUpdates.Packages.Where(p => p.Identity.Id.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1);
        }

        public async Task<IEnumerable<IPackageSearchMetadata>> GetPackagesWithUpdatesAsync(string searchText, SearchFilter searchFilter, CancellationToken cancellationToken)
        {
            var packages = _installedPackages
                .Where(p => !p.IsAutoReferenced())
                .GetEarliest()
                .Where(p => p.Id.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1)
                .OrderBy(p => p.Id);

            // Prefetch metadata for all installed packages
            var prefetch = await TaskCombinators.ThrottledAsync(
                packages,
                (p, t) => _metadataProvider.GetPackageMetadataListAsync(p.Id, searchFilter.IncludePrerelease, false, t),
                cancellationToken);

            // Flatten the result list
            var prefetchedPackages = prefetch
                .Where(p => p != null)
                .SelectMany(p => p)
                .ToArray();

            // Traverse all projects and determine packages with updates
            var packagesWithUpdates = new List<IPackageSearchMetadata>();
            foreach (var project in _projects)
            {
                var installed = await project.GetInstalledPackagesAsync(cancellationToken);
                foreach (var installedPackage in installed)
                {
                    var installedVersion = installedPackage.PackageIdentity.Version;
                    var allowedVersions = installedPackage.AllowedVersions ?? VersionRange.All;

                    // filter packages based on current package identity
                    var allPackages = prefetchedPackages
                        .Where(p => StringComparer.OrdinalIgnoreCase.Equals(
                            p.Identity.Id,
                            installedPackage.PackageIdentity.Id))
                        .ToArray();

                    // and allowed versions
                    var allowedPackages = allPackages
                        .Where(p => allowedVersions.Satisfies(p.Identity.Version));

                    // peek the highest available
                    var highest = allowedPackages
                        .OrderByDescending(e => e.Identity.Version, VersionComparer.VersionRelease)
                        .FirstOrDefault();

                    if (highest != null &&
                        VersionComparer.VersionRelease.Compare(installedVersion, highest.Identity.Version) < 0)
                    {
                        packagesWithUpdates.Add(highest.WithVersions(ToVersionInfo(allPackages)));
                    }
                }
            }

            // select the earliest package update candidates
            var uniquePackageIds = packagesWithUpdates
                .Select(p => p.Identity)
                .GetEarliest();

            // get unique list of package metadata as similar updates may come from different projects
            var uniquePackages = uniquePackageIds
                .GroupJoin(
                    packagesWithUpdates,
                    id => id,
                    p => p.Identity,
                    (id, pl) => pl.First());

            return uniquePackages.ToArray();
        }

        private static IEnumerable<VersionInfo> ToVersionInfo(IEnumerable<IPackageSearchMetadata> packages)
        {
            return packages?
                .OrderByDescending(m => m.Identity.Version, VersionComparer.VersionRelease)
                .Select(m => new VersionInfo(m.Identity.Version, m.DownloadCount)
                {
                    PackageSearchMetadata = m
                });
        }
        #endregion
    }
}
