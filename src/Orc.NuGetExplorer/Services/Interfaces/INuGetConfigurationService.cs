// --------------------------------------------------------------------------------------------------------------------
// <copyright file="INuGetConfigurationService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System.Collections.Generic;
    using NuGet.Configuration;

    public interface INuGetConfigurationService
    {
        #region Methods
        string GetDestinationFolder();
        void SetDestinationFolder(string value);
        IEnumerable<PackageSource> LoadPackageSources(bool onlyEnabled = false);

        bool SavePackageSource(string name, string source, bool isEnabled = true, bool isOfficial = true, bool verifyFeed = true);

        void DisablePackageSource(string name, string source);
        void SavePackageSources(IEnumerable<PackageSource> packageSources);
        void SetIsPrereleaseAllowed(IRepository repository, bool value);
        bool GetIsPrereleaseAllowed(IRepository repository);
        #endregion
    }
}
