﻿namespace Orc.NuGetExplorer.Providers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Catel;
    using Catel.IoC;
    using Catel.Logging;
    using NuGet.Common;
    using NuGet.Packaging.Core;
    using NuGet.Protocol.Core.Types;
    using Orc.NuGetExplorer.Management;

    public class PackageMetadataProvider : IPackageMetadataProvider
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private static readonly ILogger NuGetLogger;

        private readonly IEnumerable<SourceRepository> _sourceRepositories;

        private readonly IEnumerable<SourceRepository> _optionalLocalRepositories;

#pragma warning disable IDE0052 // Remove unread private members
        private readonly SourceRepository _localRepository;
#pragma warning restore IDE0052 // Remove unread private members

        static PackageMetadataProvider()
        {
            NuGetLogger = ServiceLocator.Default.ResolveType<ILogger>();
        }

        public PackageMetadataProvider(IRepositoryService repositoryService, ISourceRepositoryProvider repositoryProvider)
        {
            _sourceRepositories = repositoryProvider.GetRepositories();
            _optionalLocalRepositories = new[] { repositoryProvider.CreateRepository(repositoryService.LocalRepository.ToPackageSource()) };
        }

        public PackageMetadataProvider(IEnumerable<SourceRepository> sourceRepositories,
            IEnumerable<SourceRepository> optionalGlobalLocalRepositories, SourceRepository localRepository = null)
        {
            Argument.IsNotNull(() => sourceRepositories);

            _sourceRepositories = sourceRepositories;
            _optionalLocalRepositories = optionalGlobalLocalRepositories;
            _localRepository = localRepository;
        }



        public static PackageMetadataProvider CreateFromSourceContext(IRepositoryContextService repositoryService, IExtensibleProjectLocator projectSource, INuGetPackageManager projectManager)
        {
            var context = repositoryService.AcquireContext();

            var projects = projectSource.GetAllExtensibleProjects();

            var localRepos = projectManager.AsLocalRepositories(projects);

            var repos = context.Repositories ?? context.PackageSources?.Select(src => repositoryService.GetRepository(src)) ?? new List<SourceRepository>();

            return new PackageMetadataProvider(repos, localRepos);
        }


        public async Task<IPackageSearchMetadata> GetLocalPackageMetadataAsync(PackageIdentity identity, bool includePrerelease, CancellationToken cancellationToken)
        {
            var sources = new List<SourceRepository>();

            if (_optionalLocalRepositories != null)
            {
                sources.AddRange(_optionalLocalRepositories);
            }

            //if (_sourceRepositories != null)
            //{
            //    sources.AddRange(_sourceRepositories);
            //}

            // Take the package from the first source it is found in
            foreach (var source in sources)
            {
                var result = await GetPackageMetadataFromLocalSourceAsync(source, identity, cancellationToken);

                if (result != null)
                {
                    //TODO why additional fetching needed?
                    //return result.WithVersions(
                    //    () => FetchAndMergeVersionsAsync(identity, includePrerelease, ))
                    return result;
                }
            }

            return null;
        }

        public async Task<IPackageSearchMetadata> GetLowestLocalPackageMetadataAsync(string packageid, bool includePrrelease, CancellationToken cancellationToken)
        {
            var sources = new List<SourceRepository>();

            if (_optionalLocalRepositories != null)
            {
                sources.AddRange(_optionalLocalRepositories);
            }

            var tasks = sources.Select(r => GetPackageMetadataFromLocalSourceAsync(r, packageid, cancellationToken)).ToArray();

            var completed = (await tasks.WhenAllOrException()).Where(x => x.IsSuccess)
                .Select(x => x.UnwrapResult())
                .Where(metadata => metadata != null);

            var lowest = completed.SelectMany(p => p)
                .OrderBy(p => p.Identity.Version)
                .FirstOrDefault();

            return lowest;
        }


        public async Task<IPackageSearchMetadata> GetPackageMetadataAsync(PackageIdentity identity, bool includePrerelease, CancellationToken cancellationToken)
        {
            if (!_sourceRepositories.Any())
            {
                Log.Warning("No repositories available");
                return null;
            }

            var tasks = _sourceRepositories
               .Select(r => GetPackageMetadataAsyncFromSource(r, identity, includePrerelease, cancellationToken)).ToArray();

            //if (_localRepository != null)
            //{
            //    tasks.Add(_localRepository.GetPackageMetadataFromLocalSourceAsync(identity, cancellationToken));
            //}

            var completed = (await tasks.WhenAllOrException()).Where(x => x.IsSuccess)
                .Select(x => x.UnwrapResult())
                .Where(metadata => metadata != null);


            var master = completed.FirstOrDefault(m => !string.IsNullOrEmpty(m.Summary))
                ?? completed.FirstOrDefault()
                ?? PackageSearchMetadataBuilder.FromIdentity(identity).Build();

            //return master.WithVersions(
            //    asyncValueFactory: () => MergeVersionsAsync(identity, completed));

            return master;
        }

        public async Task<IPackageSearchMetadata> GetHighestPackageMetadataAsync(string packageId, bool includePrerelease, CancellationToken cancellationToken)
        {
            //returned type - packageRegistrationMetadata
            var metadataList = await GetPackageMetadataListAsync(packageId, includePrerelease, false, cancellationToken);

            var master = metadataList.OrderByDescending(x => x.Identity.Version).FirstOrDefault();

            return master?.WithVersions(() => metadataList.ToVersionInfo(includePrerelease));
        }

        public async Task<IEnumerable<IPackageSearchMetadata>> GetPackageMetadataListAsync(string packageId, bool includePrerelease, bool includeUnlisted, CancellationToken cancellationToken)
        {
            var tasks = _sourceRepositories.Select(repo => GetPackageMetadataListAsyncFromSource(repo, packageId, includePrerelease, includeUnlisted, cancellationToken)).ToArray();

            var completed = (await tasks.WhenAllOrException()).Where(x => x.IsSuccess).
                Select(x => x.UnwrapResult())
                .Where(metadata => metadata != null);

            var packages = completed.SelectMany(p => p);

            var uniquePackages = packages
                .GroupBy(
                   m => m.Identity.Version,
                   (v, ms) => ms.First());

            return uniquePackages;
        }

        /// <summary>
        /// Returns list of package metadata objects from repository
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="packageId"></param>
        /// <param name="includePrerelease"></param>
        /// <param name="includeUnlisted"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<IPackageSearchMetadata>> GetPackageMetadataListAsyncFromSource(SourceRepository repository,
            string packageId,
            bool includePrerelease,
            bool includeUnlisted,
            CancellationToken cancellationToken)
        {

            var metadataResource = await repository.GetResourceAsync<PackageMetadataResource>(cancellationToken);

            using (var sourceCacheContext = new SourceCacheContext())
            {
                //todo
                //check httpCache created inside GetMetadataAsync()
                //The Root folder value didn't used when retry count is 0
                //Then temporary folder for package never created and SourceCacheContext dispose caused
                //DirectoryNotFoundException
                //var httpCache = HttpSourceCacheContext.Create(sourceCacheContext, 0);

                // Update http source cache context MaxAge so that it can always go online to fetch
                // latest versions of the package.
                //sourceCacheContext.MaxAge = DateTimeOffset.UtcNow;

                //force creating folder for cache even http retry count is 0
                Directory.CreateDirectory(sourceCacheContext.GeneratedTempFolder);

                Log.Debug($"Get all versions metadata, creating temp {sourceCacheContext.GeneratedTempFolder}");

                var packages = await metadataResource?.GetMetadataAsync(
                    packageId,
                    includePrerelease,
                    includeUnlisted,
                    sourceCacheContext,
                    NuGetLogger,
                    cancellationToken);

                Log.Debug($"Returned package metadata count: {packages.Count()}");

                return packages;

            }
        }

        /// <summary>
        /// Returns list of package metadata objects along with all version metadtas from repository
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="identity"></param>
        /// <param name="includePrerelease"></param>
        /// <param name="takeVersions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<IPackageSearchMetadata> GetPackageMetadataAsyncFromSource(SourceRepository repository,
            PackageIdentity identity,
            bool includePrerelease,
            CancellationToken cancellationToken,
            bool takeVersions = true)
        {
            if (takeVersions)
            {
                //query all versions and pack them in a single object
                var versionsMetadatas = await GetPackageMetadataListAsyncFromSource(repository, identity.Id, includePrerelease, false, cancellationToken);

                if (!versionsMetadatas?.Any() ?? false)
                {
                    return null;
                }

                var unitedMetadata = versionsMetadatas
                    .FirstOrDefault(p => p.Identity.Version == identity.Version)
                    ?? PackageSearchMetadataBuilder.FromIdentity(identity).Build();

                return unitedMetadata.WithVersions(versionsMetadatas.ToVersionInfo(includePrerelease));
            }

            using (var sourceCacheContext = new SourceCacheContext())
            {
                var metadataResource = await repository.GetResourceAsync<PackageMetadataResource>(cancellationToken);

                sourceCacheContext.MaxAge = DateTimeOffset.UtcNow;

                var package = await metadataResource?.GetMetadataAsync(identity, sourceCacheContext, NuGetLogger, cancellationToken);
                return package;
            }
        }


        private async Task<IPackageSearchMetadata> GetPackageMetadataFromLocalSourceAsync(SourceRepository localRepository, PackageIdentity packageIdentity, CancellationToken token)
        {
            var localPackages = await GetPackageMetadataFromLocalSourceAsync(localRepository, packageIdentity.Id, token);

            var packageMetadata = localPackages?.FirstOrDefault(p => p.Identity.Version == packageIdentity.Version);

            var versions = new[]
            {
                    new VersionInfo(packageIdentity.Version)
                };

            return packageMetadata?.WithVersions(versions);

        }

        private async Task<IEnumerable<IPackageSearchMetadata>> GetPackageMetadataFromLocalSourceAsync(
            SourceRepository localRepository,
            string packageId,
            CancellationToken token)
        {
            var localResource = await localRepository.GetResourceAsync<PackageMetadataResource>(token);

            using (var sourceCacheContext = new SourceCacheContext())
            {
                var localPackages = await localResource?.GetMetadataAsync(
                    packageId,
                    includePrerelease: true,
                    includeUnlisted: true,
                    sourceCacheContext: sourceCacheContext,
                    log: NuGetLogger,
                    token: token);

                return localPackages;
            }
        }
    }
}
