// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SearchResult.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Catel.Data;
    using NuGet.Protocol.Core.Types;

    public sealed class SearchResult : SearchResult<IPackageSearchMetadata>
    {
        #region Constructors
        public SearchResult(IReadOnlyList<IPackageSearchMetadata> items)
            : base(items)
        {
        }
        #endregion

        #region Properties
        public List<IPackageSearchMetadata> PackageList => Items;
        #endregion

        #region Methods
        public static SearchResult FromItems(params IPackageSearchMetadata[] items)
        {
            return new SearchResult(items);
        }

        public static SearchResult FromItems(IReadOnlyList<IPackageSearchMetadata> items)
        {
            return new SearchResult(items);
        }

        public static SearchResult<IPackageSearchMetadata> Empty()
        {
            return new SearchResult<IPackageSearchMetadata>(new IPackageSearchMetadata[] { });
        }
        #endregion
    }

    public class SearchResult<T> : ModelBase, IEnumerable<T>
    {
        #region Constructors
        public SearchResult(IReadOnlyList<T> items)
        {
            Items = (List<T>)items;
        }
        #endregion

        #region Properties
        public List<T> Items { get; }

        public SearchCursor Cursor { get; set; }

        public RefreshToken RefreshToken { get; set; }

        public Guid? OperationId { get; set; }

        public int TotalItemsCount { get; set; }

        public IDictionary<string, SearchStatus> SearchStatusBySource { get; set; } = new Dictionary<string, SearchStatus>();

        public IDictionary<string, Exception> SearchExceptionBySource { get; set; } = new Dictionary<string, Exception>();
        #endregion

        #region Methods
        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
        #endregion
    }
}
