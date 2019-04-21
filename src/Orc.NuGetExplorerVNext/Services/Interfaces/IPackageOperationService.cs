// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPackageOperationService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using NuGet.Protocol.Core.Types;

    public interface IPackageOperationService
    {
        #region Methods
        void UninstallPackage(IPackageSearchMetadata package);
        void InstallPackage(IPackageSearchMetadata package, bool allowedPrerelease);
        void UpdatePackages(IPackageSearchMetadata package, bool allowedPrerelease);
        #endregion
    }
}
