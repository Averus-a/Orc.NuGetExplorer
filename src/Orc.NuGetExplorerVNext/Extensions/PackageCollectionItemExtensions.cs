// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageCollectionItemExtensions.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using System.Linq;
    using NuGet.ProjectManagement;

    internal static class PackageCollectionItemExtensions
    {
        public static bool IsAutoReferenced(this PackageCollectionItem package)
        {
            return package.PackageReferences
                .Select(e => e as BuildIntegratedPackageReference)
                .Any(e => e?.Dependency?.AutoReferenced == true);
        }
    }
}
