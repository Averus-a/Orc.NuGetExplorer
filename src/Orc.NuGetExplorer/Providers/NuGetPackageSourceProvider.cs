// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NuGetPackageSourceProvider.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System.Linq;
    using NuGet.Configuration;

    internal class NuGetPackageSourceProvider : PackageSourceProvider
    {
        #region Constructors
        public NuGetPackageSourceProvider(ISettings settingsManager)
            : base(settingsManager, Enumerable.Empty<PackageSource>())
        {
        }
        #endregion
    }
}
