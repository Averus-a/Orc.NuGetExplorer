// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPackageSourceFactory.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using NuGet.Configuration;

    public interface IPackageSourceFactory
    {
        #region Methods
        PackageSource CreatePackageSource(string source, string name, bool isEnabled, bool isOfficial);
        #endregion
    }
}
