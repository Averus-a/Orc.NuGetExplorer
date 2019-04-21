// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageCommandService.cs" company="WildGums">
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
    using Catel.IoC;
    using Catel.Services;
    using NuGet.Protocol.Core.Types;

    internal class PackageCommandService : IPackageCommandService
    {
        #region Fields
        private readonly IApiPackageRegistry _apiPackageRegistry;

        private readonly SourceRepository _localRepository;

        private readonly IPackageOperationContextService _packageOperationContextService;

        private readonly IPackageOperationService _packageOperationService;

        private readonly IPackageQueryService _packageQueryService;

        private readonly IPleaseWaitService _pleaseWaitService;
        #endregion

        #region Constructors
        //public PackageCommandService(IServiceLocator serviceLocator)
        //{
        //    _pleaseWaitService = serviceLocator.ResolveType<IPleaseWaitService>();

        //    _packageQueryService = serviceLocator.ResolveType<IPackageQueryService>();
        //    _packageOperationService = serviceLocator.ResolveType<IPackageOperationService>();
        //    _packageOperationContextService = serviceLocator.ResolveType<IPackageOperationContextService>();
        //    _apiPackageRegistry = serviceLocator.ResolveType<IApiPackageRegistry>();

        //    var repositoryService = serviceLocator.ResolveType<IRepositoryService>();
        //    _localRepository = repositoryService.LocalRepository;
        //}

        public PackageCommandService(IPleaseWaitService pleaseWaitService, IRepositoryService repositoryService, IPackageQueryService packageQueryService, 
            IPackageOperationService packageOperationService, IPackageOperationContextService packageOperationContextService, 
            IApiPackageRegistry apiPackageRegistry)
        {
            Argument.IsNotNull(() => pleaseWaitService);
            Argument.IsNotNull(() => packageQueryService);
            Argument.IsNotNull(() => packageOperationService);
            Argument.IsNotNull(() => packageOperationContextService);
            Argument.IsNotNull(() => apiPackageRegistry);

            _pleaseWaitService = pleaseWaitService;
            _packageQueryService = packageQueryService;
            _packageOperationService = packageOperationService;
            _packageOperationContextService = packageOperationContextService;
            _apiPackageRegistry = apiPackageRegistry;

            _localRepository = repositoryService.LocalRepository;
        }
        #endregion

        #region Methods
        public string GetActionName(PackageOperationType operationType)
        {
            return Enum.GetName(typeof(PackageOperationType), operationType);
        }

        public async Task ExecuteAsync(PackageOperationType operationType, IPackageSearchMetadata package, IRepository sourceRepository = null, bool allowedPrerelease = false)
        {
            Argument.IsNotNull(() => package);

            var selectedPackage = await GetPackageDetailsFromSelectedVersionAsync(package, sourceRepository?.SourceRepository ?? _localRepository) ?? package;

            using (_pleaseWaitService.WaitingScope())
            {
                using (_packageOperationContextService.UseOperationContext(operationType, selectedPackage))
                {
                    _packageOperationContextService.CurrentContext.Repository = sourceRepository;
                    switch (operationType)
                    {
                        case PackageOperationType.Uninstall:
                            _packageOperationService.UninstallPackage(selectedPackage);
                            break;

                        case PackageOperationType.Install:
                            _packageOperationService.InstallPackage(selectedPackage, allowedPrerelease);
                            break;

                        case PackageOperationType.Update:
                            _packageOperationService.UpdatePackages(selectedPackage, allowedPrerelease);
                            break;
                    }
                }
            }

//            package.IsInstalled = null;
        }

        public async Task<bool> CanExecuteAsync(PackageOperationType operationType, IPackageSearchMetadata package)
        {
            if (package == null)
            {
                return false;
            }

            var selectedPackage = await GetPackageDetailsFromSelectedVersionAsync(package, _localRepository) ?? package;

            switch (operationType)
            {
                case PackageOperationType.Install:
                    return await CanInstallAsync(selectedPackage);

                case PackageOperationType.Update:
                    return await CanUpdateAsync(selectedPackage);

                case PackageOperationType.Uninstall:
                    return true;
            }

            return false;
        }

        public bool IsRefreshRequired(PackageOperationType operationType)
        {
            switch (operationType)
            {
                case PackageOperationType.Uninstall:
                    return true;

                case PackageOperationType.Install:
                    return false;

                case PackageOperationType.Update:
                    return true;
            }

            return false;
        }

        public string GetPluralActionName(PackageOperationType operationType)
        {
            return $"{Enum.GetName(typeof(PackageOperationType), operationType)} all";
        }

        private async Task<IPackageSearchMetadata> GetPackageDetailsFromSelectedVersionAsync(IPackageSearchMetadata package, SourceRepository repository)
        {
            //if (!string.IsNullOrWhiteSpace(package.SelectedVersion) && package.Identity.Version.ToString() != package.SelectedVersion)
            //{
            //    package = await _packageQueryService.GetExactPackageAsync(repository, package.Id, package.SelectedVersion, CancellationToken.None);
            //}

            return package;
        }

        private async Task<bool> CanInstallAsync(IPackageSearchMetadata package)
        {
            Argument.IsNotNull(() => package);

            //if (package.IsInstalled == null)
            //{
            //    var somePackages = await _packageQueryService.GetPackagesAsync(_localRepository, package.Id, true, 1, CancellationToken.None);
            //    package.IsInstalled = somePackages.Any();
            //    await ValidatePackageAsync(package);
            //}

            //return package.IsInstalled != null && !package.IsInstalled.Value && package.ValidationContext.GetErrorCount(ValidationTags.Api) == 0;

            return true;
        }

        private Task ValidatePackageAsync(IPackageDetails package)
        {
            package.ResetValidationContext();
            
            return _apiPackageRegistry.ValidateAsync(package);
        }

        private async Task<bool> CanUpdateAsync(IPackageSearchMetadata package)
        {
            Argument.IsNotNull(() => package);

            //if (package.IsInstalled == null)
            //{
            //    var somePackages = await _packageQueryService.GetPackagesAsync(_localRepository, package.Id, true, 1, CancellationToken.None);
            //    package.IsInstalled = somePackages.Any();

            //    await ValidatePackageAsync(package);
            //}

            //return !package.IsInstalled.Value && package.ValidationContext.GetErrorCount(ValidationTags.Api) == 0;

            return true;
        }
        #endregion
    }
}
