﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackagesUpdatesSearcherService.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System.Collections.Generic;
    using System.Linq;
    using Catel;
    using NuGet;
    using Repositories;

    internal class PackagesUpdatesSearcherService : IPackagesUpdatesSearcherService
    {
        #region Fields
        private readonly IAuthenticationSilencerService _authenticationSilencerService;
        private readonly IPackageQueryService _packageQueryService;
        private readonly IPackageCacheService _packageCacheService;
        private readonly IPackageRepositoryService _packageRepositoryService;
        #endregion

        #region Constructors
        public PackagesUpdatesSearcherService(IPackageRepositoryService packageRepositoryService, IAuthenticationSilencerService authenticationSilencerService,
            IPackageQueryService packageQueryService, IPackageCacheService packageCacheService)
        {
            Argument.IsNotNull(() => packageRepositoryService);
            Argument.IsNotNull(() => authenticationSilencerService);
            Argument.IsNotNull(() => packageQueryService);

            _packageRepositoryService = packageRepositoryService;
            _authenticationSilencerService = authenticationSilencerService;
            _packageQueryService = packageQueryService;
            _packageCacheService = packageCacheService;
        }
        #endregion

        #region Methods
        public IEnumerable<IPackageDetails> SearchForUpdates(bool allowPrerelease = false, bool authenticateIfRequired = false)
        {
            using (_authenticationSilencerService.UseAuthentication(authenticateIfRequired))
            {
                var packageRepository = _packageRepositoryService.GetAggregateRepository();

                var queryable = _packageRepositoryService.LocalRepository.GetPackages();
                return packageRepository.GetUpdates(queryable, allowPrerelease, false).Select(package => _packageCacheService.GetPackageDetails(package));
            }
        }
        #endregion
    }
}