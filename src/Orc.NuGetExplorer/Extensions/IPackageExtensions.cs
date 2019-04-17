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
        public static bool IsPrerelease(this Package package)
        {
            Argument.IsNotNull(() => package);

            return !string.IsNullOrWhiteSpace(package.Version.SpecialVersion);
        }

        public static string GetKeyForCache(this Package package, bool allowPrereleaseVersions)
        {
            Argument.IsNotNull(() => package);

            return string.Format("{0}_{1}_{2}", package.GetType().Name, package.GetFullName(), allowPrereleaseVersions);
        }
        #endregion
    }
}
