// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPackageFeed.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using System.Threading;
    using System.Threading.Tasks;
    using NuGet.Protocol.Core.Types;

    public interface IPackageFeed
    {
        bool IsMultiSource { get; }

        Task<SearchResult<IPackageSearchMetadata>> SearchAsync(
            string searchText, SearchFilter filter, CancellationToken cancellationToken);

        Task<SearchResult<IPackageSearchMetadata>> ContinueSearchAsync(
            SearchCursor searchCursor, CancellationToken cancellationToken);

        Task<SearchResult<IPackageSearchMetadata>> RefreshSearchAsync(
            RefreshToken refreshToken, CancellationToken cancellationToken);
    }
}
