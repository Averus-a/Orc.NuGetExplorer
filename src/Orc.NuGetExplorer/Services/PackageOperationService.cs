// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageOperationService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Catel;
    using Catel.Threading;
    using NuGet;
    using NuGet.Common;
    using NuGet.Configuration;
    using NuGet.PackageManagement;
    using NuGet.Packaging;
    using NuGet.Packaging.Core;
    using NuGet.Packaging.PackageExtraction;
    using NuGet.Packaging.Signing;
    using NuGet.ProjectManagement;
    using NuGet.Protocol.Core.Types;
    using NuGet.Resolver;

    internal class PackageOperationService : IPackageOperationService
    {
        #region Fields
        private readonly SourceRepository _localRepository;
        private readonly ILogger _logger;
        private readonly NuGetPackageManager _packageManager;
        private readonly IPackageOperationContextService _packageOperationContextService;
        private readonly IRepositoryCacheService _repositoryCacheService;
        private readonly IApiPackageRegistry _apiPackageRegistry;
        private readonly INuGetProjectProvider _nuGetProjectProvider;
        private readonly ISettings _settings;
        private readonly ISourceRepositoryProvider _sourceRepositoryProvider;
        private readonly string _packageFolderPath;
        #endregion

        #region Constructors
        public PackageOperationService(IPackageOperationContextService packageOperationContextService, ILogger logger, NuGetPackageManager packageManager,
            IRepositoryService repositoryService, IRepositoryCacheService repositoryCacheService, IApiPackageRegistry apiPackageRegistry,
            INuGetProjectProvider nuGetProjectProvider)
        {
            Argument.IsNotNull(() => packageOperationContextService);
            Argument.IsNotNull(() => logger);
            Argument.IsNotNull(() => packageManager);
            Argument.IsNotNull(() => repositoryService);
            Argument.IsNotNull(() => repositoryCacheService);
            Argument.IsNotNull(() => apiPackageRegistry);
            Argument.IsNotNull(() => nuGetProjectProvider);

            _packageOperationContextService = packageOperationContextService;
            _logger = logger;
            _packageManager = packageManager;
            _repositoryCacheService = repositoryCacheService;
            _apiPackageRegistry = apiPackageRegistry;
            _nuGetProjectProvider = nuGetProjectProvider;

            _localRepository = repositoryCacheService.GetNuGetRepository(repositoryService.LocalRepository);
        }
        #endregion

        #region Methods
        public void UninstallPackage(IPackage package, bool removeDependencies)
        {
            Argument.IsNotNull(() => package);

#pragma warning disable 4014
            TaskHelper.Run(async delegate
#pragma warning restore 4014
            {
                var packageManager =
                    new NuGetPackageManager(
                        _sourceRepositoryProvider,
                        _settings,
                        _packageFolderPath);

                var uninstallContext = new UninstallationContext(removeDependencies, false);
                var projectContext = new ProjectContext();

                var logger = new LoggerAdapter(projectContext);
                projectContext.PackageExtractionContext = new PackageExtractionContext(
                    PackageSaveMode.Defaultv2,
                    PackageExtractionBehavior.XmlDocFileSaveMode,
                    ClientPolicyContext.GetClientPolicy(_settings, logger),
                    logger);

                var nuGetProject = _nuGetProjectProvider.GetProject();

                await packageManager.UninstallPackageAsync(nuGetProject, package.Id, uninstallContext, projectContext, CancellationToken.None);
            });
        }


        public void InstallPackage(string source, string packageId, Version version, bool ignoreDependencies)
        {

        }

        public void InstallLatestPackage(string source, string packageId, bool includePrerelease, bool ignoreDependencies)
        {

        }
        //public void InstallPackage(IPackage package, bool allowedPrerelease)
        //{
        //    Argument.IsNotNull(() => package);
        //    Argument.IsOfType(() => package, typeof (Package));

        //    var packageManager = CreatePackageManager();

        //    var resolutionContext = new ResolutionContext(DependencyBehavior.Highest, allowedPrerelease, false, VersionConstraints.None);

        //    var nuGetProject = _nuGetProjectProvider.GetProject();
        //    packageManager.InstallPackageAsync(nuGetProject, package.Id, )

        //    var repository = _packageOperationContextService.CurrentContext.Repository;
        //    var sourceRepository = _repositoryCacheService.GetNuGetRepository(repository);

        //    var walker = new InstallWalker(_localRepository, sourceRepository, null, _logger, false, allowedPrerelease, DependencyVersion);

        //    try
        //    {
        //        ValidatePackage(package);
        //        var nuGetPackage = EnsurePackageDependencies(((Package)package).SearchMetadata);
        //        walker.ResolveOperations(nuGetPackage);
        //        _packageManager.InstallPackage(nuGetPackage, false, allowedPrerelease, false);
        //    }
        //    catch (Exception exception)
        //    {
        //        _logger.Log(MessageLevel.Error, exception.Message);
        //        _packageOperationContextService.CurrentContext.CatchedExceptions.Add(exception);
        //    }
        //}

        private NuGetPackageManager CreatePackageManager()
        {
            var packageManager =
                new NuGetPackageManager(
                    _sourceRepositoryProvider,
                    _settings,
                    _packageFolderPath);
            return packageManager;
        }

        internal async Task InstallInternalAsync(
            List<PackageIdentity> packages,
            ProjectContext projectContext,
            bool includePrerelease,
            bool ignoreDependencies,
            CancellationToken token)
        {
            // Go off the UI thread. This may be called from the UI thread. Only switch to the UI thread where necessary
            // This method installs multiple packages and can likely take more than a few secs
            // So, go off the UI thread explicitly to improve responsiveness
            await TaskScheduler.Default;

            var gatherCache = new GatherCache();
            var sources = _sourceRepositoryProvider.GetRepositories().ToList();


                var depBehavior = ignoreDependencies ? DependencyBehavior.Ignore : DependencyBehavior.Lowest;

                var packageManager = CreatePackageManager();

                // find the project
                var nuGetProject = _nuGetProjectProvider.GetProject();
                var nuGetProject = await _solutionManager.GetOrCreateProjectAsync(project, projectContext);

                var packageManagementFormat = new PackageManagementFormat(_settings);
                // 1 means PackageReference
                var preferPackageReference = packageManagementFormat.SelectedPackageManagementFormat == 1;

                // Check if default package format is set to `PackageReference` and project has no
                // package installed yet then upgrade it to `PackageReference` based project.
                if(preferPackageReference &&
                   (nuGetProject is MSBuildNuGetProject) &&
                   !(await nuGetProject.GetInstalledPackagesAsync(token)).Any() &&
                   await NuGetProjectUpgradeUtility.IsNuGetProjectUpgradeableAsync(nuGetProject, project, needsAPackagesConfig: false))
                {
                    nuGetProject = await _solutionManager.UpgradeProjectToPackageReferenceAsync(nuGetProject);
                }

                // install the package
                foreach (var package in packages)
                {
                    // Check if the package is already installed
                    if (package.Version != null &&
                        _packageServices.IsPackageInstalledEx(project, package.Id, package.Version.ToString()))
                    {
                            continue;
                    }

                    // Perform the install
                    await InstallInternalCoreAsync(
                        packageManager,
                        gatherCache,
                        nuGetProject,
                        package,
                        sources,
                        projectContext,
                        includePrerelease,
                        ignoreDependencies,
                        token);
                }
            
        }

        internal async Task InstallInternalCoreAsync(
            NuGetPackageManager packageManager,
            GatherCache gatherCache,
            NuGetProject nuGetProject,
            PackageIdentity package,
            IEnumerable<SourceRepository> sources,
            ProjectContext projectContext,
            bool includePrerelease,
            bool ignoreDependencies,
            CancellationToken token)
        {
            var depBehavior = ignoreDependencies ? DependencyBehavior.Ignore : DependencyBehavior.Lowest;

            using (var sourceCacheContext = new SourceCacheContext())
            {
                var resolution = new ResolutionContext(
                    depBehavior,
                    includePrerelease,
                    includeUnlisted: false,
                    versionConstraints: VersionConstraints.None,
                    gatherCache: gatherCache,
                    sourceCacheContext: sourceCacheContext);

                // install the package
                if (package.Version == null)
                {
                    await packageManager.InstallPackageAsync(nuGetProject, package.Id, resolution, projectContext, sources, Enumerable.Empty<SourceRepository>(), token);
                }
                else
                {
                    await packageManager.InstallPackageAsync(nuGetProject, package, resolution, projectContext, sources, Enumerable.Empty<SourceRepository>(), token);
                }
            }
        }

        public void UpdatePackages(IPackage package, bool allowedPrerelease)
        {
            Argument.IsNotNull(() => package);
            Argument.IsOfType(() => package, typeof(Package));

            try
            {
                ValidatePackage(package);
                var nuGetPackage = EnsurePackageDependencies(((Package)package).SearchMetadata);
                _packageManager.UpdatePackage(nuGetPackage, true, allowedPrerelease);
            }
            catch (Exception exception)
            {
                _logger.Log(MessageLevel.Error, exception.Message);
                _packageOperationContextService.CurrentContext.CatchedExceptions.Add(exception);
            }
        }

        private void ValidatePackage(IPackage package)
        {
            package.ResetValidationContext();

            _apiPackageRegistry.Validate(package);

            if (package.ValidationContext.GetErrorCount(ValidationTags.Api) > 0)
            {
                throw new ApiValidationException(package.ValidationContext.GetErrors(ValidationTags.Api).First().Message);
            }
        }

        private PackageWrapper EnsurePackageDependencies(Package nuGetPackage)
        {
            List<PackageDependencySet> dependencySets = new List<PackageDependencySet>();
            foreach (PackageDependencySet dependencySet in nuGetPackage.DependencySets)
            {
                dependencySets.Add(new PackageDependencySet(dependencySet.TargetFramework, dependencySet.Dependencies.Where(dependency => !_apiPackageRegistry.IsRegistered(dependency.Id))));
            }

            return new PackageWrapper(nuGetPackage, dependencySets);
        }
        #endregion
    }
}
