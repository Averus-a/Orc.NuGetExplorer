﻿using Catel.IoC;
using Catel.Services;
using Orc.NuGetExplorer;

/// <summary>
/// Used by the ModuleInit. All code inside the Initialize method is ran as soon as the assembly is loaded.
/// </summary>
public static class ModuleInitializer
{
    /// <summary>
    /// Initializes the module.
    /// </summary>
    public static void Initialize()
    {
        var serviceLocator = ServiceLocator.Default;

        // Services
        //serviceLocator.RegisterType<INuGetConfigurationService, NuGetConfigurationService>();
        //serviceLocator.RegisterType<INuGetFeedVerificationService, NuGetFeedVerificationService>();
        serviceLocator.RegisterType<INuGetLogListeningSevice, NuGetLogListeningSevice>();
        //serviceLocator.RegisterType<IPackageCacheService, PackageCacheService>();
        //serviceLocator.RegisterType<IPackageOperationContextService, PackageOperationContextService>();
        //serviceLocator.RegisterType<IPackageOperationService, PackageOperationService>();
        //serviceLocator.RegisterType<IPackageQueryService, PackageQueryService>();
        //serviceLocator.RegisterType<IRepositoryService, RepositoryService>();
        //serviceLocator.RegisterType<IPackageSourceFactory, PackageSourceFactory>();
        //serviceLocator.RegisterType<IPackagesUpdatesSearcherService, PackagesUpdatesSearcherService>();
        serviceLocator.RegisterType<IRollbackPackageOperationService, RollbackPackageOperationService>();
        serviceLocator.RegisterType<IBackupFileSystemService, BackupFileSystemService>();
        serviceLocator.RegisterType<ITemporaryFIleSystemContextService, TemporaryFIleSystemContextService>();
        //serviceLocator.RegisterType<IFileSystemService, FileSystemService>();
        //serviceLocator.RegisterType<IPleaseWaitInterruptService, PleaseWaitInterruptService>();

        serviceLocator.RegisterType<IAuthenticationProvider, AuthenticationProvider>();
        serviceLocator.RegisterType<IDefaultPackageSourcesProvider, EmptyDefaultPackageSourcesProvider>();

        //var nuGetPackageManager = serviceLocator.ResolveType<IPackageManager>();
        //serviceLocator.RegisterInstance(typeof(IPackageOperationNotificationService), nuGetPackageManager);

        var typeFactory = serviceLocator.ResolveType<ITypeFactory>();

        serviceLocator.RegisterTypeAndInstantiate<DeletemeWatcher>();
        serviceLocator.RegisterTypeAndInstantiate<RollbackWatcher>();
        serviceLocator.RegisterTypeAndInstantiate<NuGetToCatelLogTranslator>();

        var languageService = serviceLocator.ResolveType<ILanguageService>();
        languageService.RegisterLanguageSource(new LanguageResourceSource("Orc.NuGetExplorer", "Orc.NuGetExplorer.Properties", "Resources"));

        //serviceLocator.RegisterType<IApiPackageRegistry, ApiPackageRegistry>();
    }
}
