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
    using System.Linq;
    using Catel.Data;

    public sealed class SearchResult : SearchResult<IPackageDetails>
    {
        public SearchResult(IReadOnlyList<IPackageDetails> items) 
            : base(items)
        {
        }
    }

    public class SearchResult<T> : ModelBase, IEnumerable<T>
    {
        public SearchResult(IReadOnlyList<T> items)
        {
            PackageList = (List<T>)items;
        }

        public List<T> PackageList { get; private set; }

        public SearchCursor Cursor { get; set; }

        public bool CanContinue { get; private set; } //=> SearchStatusBySource.Values.Any(x => x != SearchStatus.NoMoreFound && x != SearchStatus.NothingFound);

        public IEnumerator<T> GetEnumerator()
        {
            return PackageList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return PackageList.GetEnumerator();
        }

        public IDictionary<string, SearchStatus> SearchStatusBySource { get; set; } = new Dictionary<string, SearchStatus>();

        public IDictionary<string, Exception> SearchExceptionBySource { get; set; } = new Dictionary<string, Exception>();
    }
}
