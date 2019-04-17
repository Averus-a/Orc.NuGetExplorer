// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageCacheService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System.Linq;
    using Catel;
    using Catel.Caching;
    using NuGet;
    using NuGet.Protocol.Core.Types;

    internal class PackageCacheService : IPackageCacheService
    {
        #region Fields
        private readonly ICacheStorage<string, Package> _packageDetailsCache = new CacheStorage<string, Package>();
        #endregion

        #region Constructors
        public PackageCacheService()
        {
        }
        #endregion

        #region Methods
        public Package GetPackageDetails(SourceRepository packageRepository, Package package, bool allowPrereleaseVersions)
        {
            Argument.IsNotNull(() => package);

            return _packageDetailsCache.GetFromCacheOrFetch(package.GetKeyForCache(allowPrereleaseVersions), () => new Package(package, packageRepository.FindPackagesById(package.Id).Select(p => p.Version.ToString()).Where(p => allowPrereleaseVersions || !p.Contains("-"))));
        }
        #endregion
    }
}
