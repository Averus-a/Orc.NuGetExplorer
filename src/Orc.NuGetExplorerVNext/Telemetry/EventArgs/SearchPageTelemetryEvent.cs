// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SearchPageTelemetryEvent.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer.Telemetry
{
    using System;
    using NuGet.Common;

    public class SearchPageTelemetryEvent : TelemetryEvent
    {
        public SearchPageTelemetryEvent(
            Guid parentId,
            int pageIndex,
            int resultCount,
            TimeSpan duration,
            SearchStatus loadingStatus) : base("SearchPage")
        {
            base["ParentId"] = parentId.ToString();
            base["PageIndex"] = pageIndex;
            base["ResultCount"] = resultCount;
            base["Duration"] = duration.TotalSeconds;
            base["LoadingStatus"] = loadingStatus.ToString();
        }
    }
}
