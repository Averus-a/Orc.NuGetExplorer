// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SearchStatusExtensions.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using System.Collections.Generic;
    using System.Linq;

    public static class SearchStatusExtensions
    {
        public static SearchStatus Aggregate(this ICollection<SearchStatus> statuses)
        {
            var count = statuses?.Count() ?? 0;

            if (count == 0)
            {
                return SearchStatus.Loading;
            }

            var first = statuses.First();
            if (count == 1 || statuses.All(x => x == first))
            {
                return first;
            }

            if (statuses.Contains(SearchStatus.Loading))
            {
                return SearchStatus.Loading;
            }

            if (statuses.Contains(SearchStatus.Failed))
            {
                return SearchStatus.Failed;
            }

            if (statuses.Contains(SearchStatus.Cancelled))
            {
                return SearchStatus.Cancelled;
            }

            if (statuses.Contains(SearchStatus.Ready))
            {
                return SearchStatus.Ready;
            }

            if (statuses.Contains(SearchStatus.NoMoreFound))
            {
                return SearchStatus.NoMoreFound;
            }

            if (statuses.Contains(SearchStatus.NothingFound))
            {
                return SearchStatus.NothingFound;
            }

            return first;
        }
    }
}
