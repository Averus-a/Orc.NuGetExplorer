// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageOperationNotificationService.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using System;
    using Catel;
    using NuGet.Protocol.Core.Types;

    public class PackageOperationNotificationService : IPackageOperationNotificationService
    {
        #region Methods
        public void NotifyOperationStarting(string installPath, PackageOperationType operationType, IPackageSearchMetadata package)
        {
            Argument.IsNotNull(() => package);

            OperationStarting?.Invoke(this, new PackageOperationEventArgs(package, installPath, operationType));
        }

        public void NotifyOperationFinished(string installPath, PackageOperationType operationType, IPackageSearchMetadata package)
        {
            Argument.IsNotNull(() => package);

            OperationFinished?.Invoke(this, new PackageOperationEventArgs(package, installPath, operationType));
        }

        public void NotifyOperationBatchStarting(PackageOperationType operationType, params IPackageSearchMetadata[] packages)
        {
            Argument.IsNotNullOrEmptyArray(() => packages);

            OperationsBatchStarting?.Invoke(this, new PackageOperationBatchEventArgs(operationType, packages));
        }

        public void NotifyOperationBatchFinished(PackageOperationType operationType, params IPackageSearchMetadata[] packages)
        {
            Argument.IsNotNullOrEmptyArray(() => packages);

            OperationsBatchFinished?.Invoke(this, new PackageOperationBatchEventArgs(operationType, packages));
        }

        public event EventHandler<PackageOperationBatchEventArgs> OperationsBatchStarting;
        public event EventHandler<PackageOperationBatchEventArgs> OperationsBatchFinished;
        public event EventHandler<PackageOperationEventArgs> OperationStarting;
        public event EventHandler<PackageOperationEventArgs> OperationFinished;
        #endregion
    }
}
