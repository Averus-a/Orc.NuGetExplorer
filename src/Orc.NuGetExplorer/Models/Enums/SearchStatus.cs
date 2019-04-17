// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SearchStatus.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    public enum SearchStatus
    {
        Unknown,
        Cancelled,
        Failed,
        Loading,
        NothingFound,
        NoMoreFound,
        Ready
    }
}
