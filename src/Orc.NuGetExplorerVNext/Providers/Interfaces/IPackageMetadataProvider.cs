// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPackageMetadataProvider.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using NuGet.Packaging.Core;
    using NuGet.ProjectManagement;
    using NuGet.Protocol.Core.Types;

    public interface IPackageMetadataProvider
    {
        Task<IPackageSearchMetadata> GetPackageMetadataAsync(PackageIdentity identity,
            bool includePrerelease, CancellationToken cancellationToken);

        Task<IPackageSearchMetadata> GetLatestPackageMetadataAsync(PackageIdentity identity, 
            NuGetProject project, bool includePrerelease, CancellationToken cancellationToken);

        Task<IEnumerable<IPackageSearchMetadata>> GetPackageMetadataListAsync(string packageId,
            bool includePrerelease, bool includeUnlisted, CancellationToken cancellationToken);

        Task<IPackageSearchMetadata> GetLocalPackageMetadataAsync(PackageIdentity identity,
            bool includePrerelease, CancellationToken cancellationToken);
    }
}
