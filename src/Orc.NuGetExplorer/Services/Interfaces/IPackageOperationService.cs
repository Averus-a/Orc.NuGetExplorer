// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPackageOperationService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    public interface IPackageOperationService
    {
        #region Methods
        void UninstallPackage(IPackage package, bool removeDependencies);
        void InstallPackage(IPackage package, bool allowedPrerelease);
        void UpdatePackages(IPackage package, bool allowedPrerelease);
        #endregion
    }
}
