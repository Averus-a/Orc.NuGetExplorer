// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageQueryService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System.Threading;
    using System.Threading.Tasks;
    using NuGet.Protocol.Core.Types;

    internal class PackageQueryService : IPackageQueryService
    {
        public Task<IPackageSearchMetadata> GetExactPackageAsync(SourceRepository repository, string packageId, string version, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<SearchResult> GetPackagesAsync(SourceRepository repository, string searchText, bool includePrerelease, int pageSize, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<SearchResult> GetPackagesAsync(SourceRepository repository, SearchCursor searchCursor, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
