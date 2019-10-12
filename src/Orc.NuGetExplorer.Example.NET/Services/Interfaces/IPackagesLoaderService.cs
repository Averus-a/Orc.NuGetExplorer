﻿using Orc.NuGetExplorer.Pagination;

namespace Orc.NuGetExplorer.Services
{
    using NuGet.Protocol.Core.Types;
    using NuGetExplorer.Pagination;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IPackagesLoaderService
    {
        System.Lazy<Providers.IPackageMetadataProvider> PackageMetadataProvider { get; set; }

        Task<IEnumerable<IPackageSearchMetadata>> LoadAsync(string searchTerm, PageContinuation pageContinuation, SearchFilter searchFilter, CancellationToken token);
    }
}