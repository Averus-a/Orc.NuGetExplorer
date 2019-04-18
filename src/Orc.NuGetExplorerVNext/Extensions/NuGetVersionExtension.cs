// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NuGetVersionExtension.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using System.Collections.Generic;
    using System.Linq;
    using NuGet.Versioning;

    public static class NuGetVersionExtension
    {
        public static NuGetVersion MinOrDefault(this IEnumerable<NuGetVersion> versions)
        {
            return versions
                .OrderBy(v => v, VersionComparer.Default)
                .FirstOrDefault();
        }

        public static NuGetVersion MaxOrDefault(this IEnumerable<NuGetVersion> versions)
        {
            return versions
                .OrderByDescending(v => v, VersionComparer.Default)
                .FirstOrDefault();
        }
    }
}
