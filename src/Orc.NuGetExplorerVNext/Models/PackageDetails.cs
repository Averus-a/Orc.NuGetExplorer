// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Package.cs" company="WildGums">
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
    using Catel.Data;
    using NuGet.Common;
    using NuGet.Frameworks;
    using NuGet.Protocol.Core.Types;

    public class PackageDetails : ModelBase, IPackageDetails
    {
        #region Fields
        private readonly SourceRepository _sourceRepository;
        #endregion

        #region Constructors
        internal PackageDetails(IPackageSearchMetadata searchMetadata, SourceRepository sourceRepository)
        {
            Argument.IsNotNull(() => searchMetadata);
            Argument.IsNotNull(() => sourceRepository);

            _sourceRepository = sourceRepository;

            SearchMetadata = searchMetadata;
            Version = searchMetadata.Identity.Version.Version;

            var id = searchMetadata.Identity.Id;

            Id = id;
            Title = string.IsNullOrWhiteSpace(searchMetadata.Title) ? id : searchMetadata.Title;
            FullName = searchMetadata.Title;
            Description = searchMetadata.Description;
            IconUrl = searchMetadata.IconUrl;

            Published = searchMetadata.Published;
            DownloadCount = searchMetadata.DownloadCount;

            OriginalVersion = searchMetadata.Identity.Version.OriginalVersion;
            Release = searchMetadata.Identity.Version.Release;

            ValidationContext = new ValidationContext();
            IsPrerelease = searchMetadata.Identity.Version.IsPrerelease;

            Authors = searchMetadata.Authors;

            AvailableVersions = new AsyncLazy<IReadOnlyList<VersionInfo>>(GetAllVersionsAsync);
            Dependencies = new AsyncLazy<IReadOnlyList<SourcePackageDependencyInfo>>(GetAllDependenciesAsync);
        }
        #endregion

        #region Properties
        public string SelectedVersion { get; set; }

        public IValidationContext ValidationContext { get; private set; }

        public string Id { get; }

        public string Title { get; }

        public string Authors { get; }

        public long? DownloadCount { get; }

        public AsyncLazy<IReadOnlyList<SourcePackageDependencyInfo>>  Dependencies { get; }

        public bool? IsInstalled { get; set; }

        public AsyncLazy<IReadOnlyList<VersionInfo>> AvailableVersions { get; }

        public string FullName { get; }

        public string Description { get; }

        public Uri IconUrl { get; }

        public IPackageSearchMetadata SearchMetadata { get; }

        public DateTimeOffset? Published { get; }

        public Version Version { get; }

        public string OriginalVersion { get; }

        public string Release { get; }

        public bool IsPrerelease { get; }
        #endregion

        #region Methods
        public void ResetValidationContext()
        {
            ValidationContext = new ValidationContext();
        }

        private async Task<IReadOnlyList<VersionInfo>> GetAllVersionsAsync()
        {
            var versionInfos = await SearchMetadata.GetVersionsAsync();

            return versionInfos.ToList();
        }

        private async Task<IReadOnlyList<SourcePackageDependencyInfo>> GetAllDependenciesAsync()
        {
            var dependencyInfoResource = await _sourceRepository.GetResourceAsync<DependencyInfoResource>();

            using (var sourceCacheContext = new SourceCacheContext())
            {
                sourceCacheContext.MaxAge = DateTimeOffset.UtcNow;

                var dependencyInfos = await dependencyInfoResource.ResolvePackages(
                    Id, 
                    NuGetFramework.AnyFramework, 
                    sourceCacheContext, 
                    NullLogger.Instance, 
                    CancellationToken.None);

                return dependencyInfos.ToList();
            }
        }
        #endregion
    }
}
