// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPackageQueryService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System.Threading;
    using System.Threading.Tasks;
    using NuGet.Protocol.Core.Types;

    public interface IPackageQueryService
    {
        #region Methods
        Task<IPackageDetails> GetExactPackageAsync(SourceRepository repository, string packageId, string version, CancellationToken cancellationToken);
        Task<SearchResult> GetPackagesAsync(SourceRepository repository, string searchText, bool includePrerelease, int pageSize, CancellationToken cancellationToken);
        Task<SearchResult> GetPackagesAsync(SourceRepository repository, SearchCursor searchCursor, CancellationToken cancellationToken);
        #endregion
    }
}
