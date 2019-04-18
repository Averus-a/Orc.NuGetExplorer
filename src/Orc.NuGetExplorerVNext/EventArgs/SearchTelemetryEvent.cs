// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SearchTelemetryEvent.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using System;
    using NuGet.Common;

    public class SearchTelemetryEvent : TelemetryEvent
    {
        public SearchTelemetryEvent(Guid operationId, string query, bool includePrerelease) : base("Search")
        {
            base["OperationId"] = operationId.ToString();
            AddPiiData("Query", query);
            base["IncludePrerelease"] = includePrerelease;
        }
    }
}
