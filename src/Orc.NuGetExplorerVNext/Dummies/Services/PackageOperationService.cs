// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageOperationService.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using NuGet.Protocol.Core.Types;

    public class PackageOperationService : IPackageOperationService
    {
        public void UninstallPackage(IPackageSearchMetadata package)
        {
            throw new System.NotImplementedException();
        }

        public void InstallPackage(IPackageSearchMetadata package, bool allowedPrerelease)
        {
            throw new System.NotImplementedException();
        }

        public void UpdatePackages(IPackageSearchMetadata package, bool allowedPrerelease)
        {
            throw new System.NotImplementedException();
        }
    }
}
