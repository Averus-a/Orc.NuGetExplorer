// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPackageExtensions.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using Catel;
    using NuGet;
    using NuGet.Packaging.Core;

    internal static class IPackageExtensions
    {
        #region Methods
        public static bool IsPrerelease(this PackageIdentity package)
        {
            Argument.IsNotNull(() => package);

            return package.Version.IsPrerelease;
        }

        public static string GetKeyForCache(this PackageIdentity package, bool allowPrereleaseVersions)
        {
            Argument.IsNotNull(() => package);

            return $"{package.GetType().Name}_{package.Version.Version}_{package.Version.Revision}";
        }
        #endregion
    }
}
