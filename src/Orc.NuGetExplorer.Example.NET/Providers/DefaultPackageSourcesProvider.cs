// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultPackageSourcesProvider.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer.Example
{
    using System.Collections.Generic;
    using System.Linq;
    using NuGet.Configuration;

    public class DefaultPackageSourcesProvider : IDefaultPackageSourcesProvider
    {
        #region Methods
        public IEnumerable<PackageSource> GetDefaultPackages()
        {
            return Enumerable.Empty<PackageSource>();
        }
        #endregion
    }
}
