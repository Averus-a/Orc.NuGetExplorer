using Catel.IoC;
using Catel.Services;
using Orc.NuGetExplorer;
using NuGet;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

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
        serviceLocator.RegisterType<INuGetLogListeningService, NuGetLogListeningService>();
        serviceLocator.RegisterType<INuGetConfigurationService, NuGetConfigurationService>();
        serviceLocator.RegisterType<INuGetFeedVerificationService, NuGetFeedVerificationService>();
        serviceLocator.RegisterType<IPackagesUpdatesSearcherService, PackagesUpdatesSearcherService>();
        serviceLocator.RegisterType<INuGetFeedVerificationService, NuGetFeedVerificationService>();
        serviceLocator.RegisterType<IPackageQueryService, PackageQueryService>();
        serviceLocator.RegisterType<IRepositoryService, RepositoryService>();
        serviceLocator.RegisterType<IPackageOperationService, PackageOperationService>();
        serviceLocator.RegisterType<IPackageOperationContextService, PackageOperationContextService>();
        serviceLocator.RegisterType<IPackageOperationNotificationService, PackageOperationNotificationService>();
        serviceLocator.RegisterType<IFileSystemService, FileSystemService>();
        serviceLocator.RegisterType<IApiPackageRegistry, ApiPackageRegistry>();

        //NuGet native
        serviceLocator.RegisterType<IPackageSourceProvider, PackageSourceProvider>();
        serviceLocator.RegisterType<ISettings, NuGetSettings>();
        serviceLocator.RegisterType<ISourceRepositoryProvider, CachingSourceProvider>();
        

        var languageService = serviceLocator.ResolveType<ILanguageService>();
        languageService.RegisterLanguageSource(new LanguageResourceSource("Orc.NuGetExplorerVNext", "Orc.NuGetExplorerVNext.Properties", "Resources"));
    }
}
