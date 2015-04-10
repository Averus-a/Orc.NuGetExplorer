﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageSourceExtensions.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System.Collections.Generic;
    using System.Linq;
    using Catel;
    using NuGet;

    public static class PackageSourceExtensions
    {
        #region Methods
        internal static IEnumerable<PackageSource> ToPackageSourceInstances(this IEnumerable<IPackageSource> packageSources)
        {
            Argument.IsNotNull(() => packageSources);

            return packageSources.Select(x => new PackageSource(x.Source, x.Name, x.IsEnabled, x.IsOfficial));
        }

        internal static IEnumerable<IPackageSource> ToPackageSourceInterfaces(this IEnumerable<PackageSource> packageSources)
        {
            Argument.IsNotNull(() => packageSources);

            return packageSources.Select(x => new NuGetPackageSource(x.Source, x.Name, x.IsEnabled, x.IsOfficial));
        }
        #endregion
    }
}