// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPackageDetails.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System;
    using System.Collections.Generic;
    using Catel.Data;
    using NuGet.Common;
    using NuGet.Protocol.Core.Types;

    public interface IPackageDetails
    {
        #region Properties
        string Id { get; }

        string FullName { get; }

        string Description { get; }

        Uri IconUrl { get; }

        Version Version { get; }

        string OriginalVersion { get; }

        bool IsPrerelease { get; }

        string Title { get; }

        string Authors { get; }

        DateTimeOffset? Published { get; }

        long? DownloadCount { get; }

        AsyncLazy<IReadOnlyList<SourcePackageDependencyInfo>> Dependencies { get; }

        bool? IsInstalled { get; set; }

        AsyncLazy<IReadOnlyList<VersionInfo>> AvailableVersions { get; }

        string SelectedVersion { get; set; }

        IValidationContext ValidationContext { get; }
        #endregion

        #region Methods
        void ResetValidationContext();
        #endregion
    }
}
