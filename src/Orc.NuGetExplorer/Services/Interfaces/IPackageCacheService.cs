// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPackageCacheService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using NuGet;
    using NuGet.Packaging.Core;
    using NuGet.Protocol.Core.Types;

    internal interface IPackageCacheService
    {
        #region Methods
        Package GetPackageDetails(SourceRepository packageRepository, Package package, bool allowPrereleaseVersions);
        #endregion
    }
}
