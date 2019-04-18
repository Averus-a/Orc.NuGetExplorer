// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDefaultPackageSourcesProvider.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System.Collections.Generic;
    using NuGet.Configuration;

    public interface IDefaultPackageSourcesProvider
    {
        #region Methods
        IEnumerable<PackageSource> GetDefaultPackages();
        #endregion
    }
}
