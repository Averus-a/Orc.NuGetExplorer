// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageQueryService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Catel;
    using Catel.Logging;
    using NuGet.Protocol.Core.Types;

    internal class PackageQueryService : IPackageQueryService
    {
        #region Fields
        private const int PageSize = 25;
        
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly IRepositoryCacheService _repositoryCacheService;
        #endregion

        #region Constructors
        public PackageQueryService(IPackageCacheService packageCacheService, IRepositoryCacheService repositoryCacheService)
        {
            Argument.IsNotNull(() => packageCacheService);
            Argument.IsNotNull(() => repositoryCacheService);

            _repositoryCacheService = repositoryCacheService;
        }
        #endregion

        #region Methods
        public async Task<IPackage> GetExactPackage(string packageId, string version, IRepository packageRepository, CancellationToken cancellationToken)
        {
            Argument.IsNotNull(() => packageRepository);

            var nuGetRepository = _repositoryCacheService.GetNuGetRepository(packageRepository);

            var packageDataList = await nuGetRepository.GetPackageDataListAsync(packageId, true, true, cancellationToken);

            return packageDataList.FirstOrDefault(x => string.Equals(x.OriginalVersion, version));
        }

        public async Task<SearchResult<IPackage>> GetPackagesAsync(string searchText, bool includePrerelease, IRepository packageRepository, CancellationToken cancellationToken)
        {
            Argument.IsNotNull(() => packageRepository);

            try
            {
                Log.Debug($"Getting {PageSize} packages starting from {0}, which contains \'{searchText}\'");
                var nuGetRepository = _repositoryCacheService.GetNuGetRepository(packageRepository);

                var filter = new SearchFilter(includePrerelease);

                return await nuGetRepository.SearchAsync(searchText, filter, PageSize, cancellationToken);
            }
            catch (Exception exception)
            {
                Log.Warning(exception);

                return new SearchResult<IPackage>(new IPackage[0]);
            }
        }

        public async Task<SearchResult<IPackage>> GetPackagesAsync(SearchCursor searchCursor, IRepository packageRepository, CancellationToken cancellationToken)
        {
            Argument.IsNotNull(() => packageRepository);

            try
            {
                Log.Debug($"Getting {PageSize} packages starting from {searchCursor.StartIndex}, which contains \'{searchCursor.SearchString}\'");
                var nuGetRepository = _repositoryCacheService.GetNuGetRepository(packageRepository);

                return await nuGetRepository.SearchAsync(searchCursor, PageSize, cancellationToken);
            }
            catch (Exception exception)
            {
                Log.Warning(exception);

                return new SearchResult<IPackage>(new IPackage[0]);
            }
        }
        #endregion
    }
}
