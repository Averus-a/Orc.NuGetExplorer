// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackagesUpdatesSearcherService.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using System.Collections.Generic;

    public class PackagesUpdatesSearcherService : IPackagesUpdatesSearcherService
    {
        public IEnumerable<IPackageDetails> SearchForUpdates(bool? allowPrerelease = null, bool authenticateIfRequired = true)
        {
            throw new System.NotImplementedException();
        }
    }
}
