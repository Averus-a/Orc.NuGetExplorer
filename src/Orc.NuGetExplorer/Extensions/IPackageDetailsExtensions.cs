// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPackageDetailsExtensions.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using NuGet;
    using NuGet.Packaging.Core;

    internal static class IPackageDetailsExtensions
    {
        #region Methods
        public static PackageIdentity ToNuGetPackage(this IPackageDetails package)
        {
            return ((PackageDetails) package).Package;
        }
        #endregion
    }
}
