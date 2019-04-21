// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SourceTelemetry.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer.Telemetry
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using NuGet.Common;
    using NuGet.Configuration;

    public static class SourceTelemetry
    {
        [Flags]
        private enum HttpStyle
        {
            NotPresent = 0,

            /// <summary>
            /// This is not set for the "microsoftdotnet" nuget.org curated feed.
            /// </summary>
            YesV2 = 1,

            YesV3 = 2,

            YesV3AndV2 = YesV3 | YesV2,
        }

        public static TelemetryEvent GetRestoreSourceSummaryEvent(
            Guid parentId,
            IEnumerable<PackageSource> packageSources)
        {
            return GetSourceSummaryEvent(
                "RestorePackageSourceSummary",
                parentId,
                packageSources);
        }

        public static TelemetryEvent GetSearchSourceSummaryEvent(
            Guid parentId,
            IEnumerable<PackageSource> packageSources)
        {
            return GetSourceSummaryEvent(
                "SearchPackageSourceSummary",
                parentId,
                packageSources);
        }

        /// <summary>
        /// Create a SourceSummaryEvent event with counts of local vs http and v2 vs v3 feeds.
        /// </summary>
        private static TelemetryEvent GetSourceSummaryEvent(
            string eventName,
            Guid parentId,
            IEnumerable<PackageSource> packageSources)
        {
            var local = 0;
            var httpV2 = 0;
            var httpV3 = 0;

            if (packageSources == null)
            {
                return new SourceSummaryTelemetryEvent(
                    eventName,
                    parentId,
                    local,
                    httpV2,
                    httpV3);
            }

            foreach (var source in packageSources)
            {
                // Ignore disabled sources
                if (!source.IsEnabled)
                {
                    continue;
                }

                if (!source.IsHttp)
                {
                    local++;
                    continue;
                }

                if (IsHttpV3(source))
                {
                    // Http V3 feed
                    httpV3++;
                }
                else
                {
                    // Http V2 feed
                    httpV2++;
                }
            }

            return new SourceSummaryTelemetryEvent(
                eventName,
                parentId,
                local,
                httpV2,
                httpV3);
        }

        /// <summary>
        /// True if the source is http and ends with index.json
        /// </summary>
        private static bool IsHttpV3(PackageSource source)
        {
            return source.IsHttp &&
                (source.Source.EndsWith("index.json", StringComparison.OrdinalIgnoreCase)
                || source.ProtocolVersion == 3);
        }

        // NumLocalFeeds(c:\ or \\ or file:///)
        // NumHTTPv2Feeds
        // NumHTTPv3Feeds
        // NuGetOrg: [NotPresent | YesV2 | YesV3]
        // VsOfflinePackages: [true | false]
        // DotnetCuratedFeed: [true | false]
        private class SourceSummaryTelemetryEvent : TelemetryEvent
        {
            public SourceSummaryTelemetryEvent(
                string eventName,
                Guid parentId,
                int local,
                int httpV2,
                int httpV3)
                : base(eventName)
            {
                this["NumLocalFeeds"] = local;
                this["NumHTTPv2Feeds"] = httpV2;
                this["NumHTTPv3Feeds"] = httpV3;
                this["ParentId"] = parentId.ToString();
            }
        }
    }
}
