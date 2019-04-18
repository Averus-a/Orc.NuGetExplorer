// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageSearchMetadataCache.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using System.Collections.Generic;
    using NuGet.Protocol.Core.Types;

    public class PackageSearchMetadataCache
    {
        // Cached Package Metadata
        public IReadOnlyList<IPackageSearchMetadata> Packages { get; set; }

        // Remember the IncludePrerelease setting corresponding to the Cached Metadata
        public bool IncludePrerelease { get; set; }
    }
}
