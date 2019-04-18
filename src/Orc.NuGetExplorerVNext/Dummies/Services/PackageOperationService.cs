// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageOperationService.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    public class PackageOperationService : IPackageOperationService
    {
        public void UninstallPackage(IPackageDetails package)
        {
            throw new System.NotImplementedException();
        }

        public void InstallPackage(IPackageDetails package, bool allowedPrerelease)
        {
            throw new System.NotImplementedException();
        }

        public void UpdatePackages(IPackageDetails package, bool allowedPrerelease)
        {
            throw new System.NotImplementedException();
        }
    }
}
