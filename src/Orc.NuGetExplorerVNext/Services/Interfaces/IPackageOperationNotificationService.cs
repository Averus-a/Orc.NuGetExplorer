// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPackageOperationNotificationService.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using System;
    using NuGet.Protocol.Core.Types;

    public interface IPackageOperationNotificationService
    {
        #region Methods
        void NotifyOperationBatchStarting(PackageOperationType operationType, params IPackageSearchMetadata[] packages);
        void NotifyOperationBatchFinished(PackageOperationType operationType, params IPackageSearchMetadata[] packages);
        void NotifyOperationFinished(string installPath, PackageOperationType operationType, IPackageSearchMetadata package);
        void NotifyOperationStarting(string installPath, PackageOperationType operationType, IPackageSearchMetadata package);
        #endregion

        #region Events
        event EventHandler<PackageOperationBatchEventArgs> OperationsBatchStarting;
        event EventHandler<PackageOperationBatchEventArgs> OperationsBatchFinished;
        event EventHandler<PackageOperationEventArgs> OperationStarting;
        event EventHandler<PackageOperationEventArgs> OperationFinished;
        #endregion
    }
}
