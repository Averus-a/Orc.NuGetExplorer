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

    public sealed class SearchResult<T> : IEnumerable<T>
    {
        public SearchResult(IReadOnlyList<T> items)
        {
            Items = items;
        }

        public IReadOnlyList<T> Items { get; }

        public SearchCursor Cursor { get; set; }

        public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

        public IDictionary<string, SearchStatus> SearchStatusBySource { get; set; } = new Dictionary<string, SearchStatus>();

        public IDictionary<string, Exception> SearchExceptionBySource { get; set; } = new Dictionary<string, Exception>();
    }
}
