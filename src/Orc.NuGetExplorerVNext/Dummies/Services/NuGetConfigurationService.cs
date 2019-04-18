// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NuGetConfigurationService.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using System.Collections.Generic;
    using System.Linq;
    using Catel;
    using NuGet.Configuration;

    public class NuGetConfigurationService : INuGetConfigurationService
    {
        private readonly IPackageSourceProvider _packageSourceProvider;

        public NuGetConfigurationService(IPackageSourceProvider packageSourceProvider)
        {
            Argument.IsNotNull(() => packageSourceProvider);

            _packageSourceProvider = packageSourceProvider;
        }

        public string GetDestinationFolder()
        {
            throw new System.NotImplementedException();
        }

        public void SetDestinationFolder(string value)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<PackageSource> LoadPackageSources(bool onlyEnabled = false)
        {
            var packageSources = _packageSourceProvider.LoadPackageSources();

            if (onlyEnabled)
            {
                packageSources = packageSources.Where(x => x.IsEnabled);
            }

            return packageSources;
        }

        public bool SavePackageSource(string name, string source, bool isEnabled = true, bool isOfficial = true, bool verifyFeed = true)
        {
            throw new System.NotImplementedException();
        }

        public void DisablePackageSource(string name, string source)
        {
            throw new System.NotImplementedException();
        }

        public void SavePackageSources(IEnumerable<PackageSource> packageSources)
        {
            throw new System.NotImplementedException();
        }

        public void SetIsPrereleaseAllowed(IRepository repository, bool value)
        {
            throw new System.NotImplementedException();
        }

        public bool GetIsPrereleaseAllowed(IRepository repository)
        {
            throw new System.NotImplementedException();
        }
    }
}
