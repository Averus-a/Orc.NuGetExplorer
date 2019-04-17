// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPackageDetailsExtensions.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using NuGet;
    using NuGet.Packaging.Core;
    using NuGet.Protocol.Core.Types;

    internal static class IPackageDetailsExtensions
    {
        #region Methods
        public static IPackageSearchMetadata ToNuGetPackage(this IPackage package)
        {
            return ((Package) package).SearchMetadata;
        }
        #endregion
    }
}
