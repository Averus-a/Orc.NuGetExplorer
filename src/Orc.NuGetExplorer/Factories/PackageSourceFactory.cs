// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageSourceFactory.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using NuGet.Configuration;

    internal class PackageSourceFactory : IPackageSourceFactory
    {
        #region Methods
        public PackageSource CreatePackageSource(string source, string name, bool isEnabled, bool isOfficial)
        {
            return new PackageSource(source, name, isEnabled, isOfficial);
        }
        #endregion
    }
}
