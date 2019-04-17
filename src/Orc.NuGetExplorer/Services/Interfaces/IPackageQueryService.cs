// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPackageQueryService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IPackageQueryService
    {
        #region Methods
        Task<IPackage> GetExactPackage(string packageId, string version, IRepository packageRepository, CancellationToken cancellationToken);
        Task<SearchResult<IPackage>> GetPackagesAsync(string searchText, bool includePrerelease, IRepository packageRepository, CancellationToken cancellationToken);
        Task<SearchResult<IPackage>> GetPackagesAsync(SearchCursor searchCursor, IRepository packageRepository, CancellationToken cancellationToken);
        #endregion
    }
}
