// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SingleSourcePackageFeedBase.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using System.Threading;
    using System.Threading.Tasks;
    using NuGet.Protocol.Core.Types;

    public abstract class SingleSourcePackageFeedBase : IPackageFeed
    {
        #region Properties
        public int PageSize { get; protected set; } = 100;

        public bool IsMultiSource => false;
        #endregion

        #region Methods
        public Task<SearchResult<IPackageSearchMetadata>> SearchAsync(string searchText, SearchFilter searchFilter, CancellationToken cancellationToken)
        {
            var searchCursor = new SearchCursor
            {
                SearchString = searchText,
                SearchFilter = searchFilter,
                StartIndex = 0
            };

            return ContinueSearchAsync(searchCursor, cancellationToken);
        }

        public abstract Task<SearchResult<IPackageSearchMetadata>> ContinueSearchAsync(SearchCursor searchCursor, CancellationToken cancellationToken);

        public Task<SearchResult<IPackageSearchMetadata>> RefreshSearchAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
        {
            return Task.FromResult(SearchResult.Empty());
        }
        #endregion
    }
}
