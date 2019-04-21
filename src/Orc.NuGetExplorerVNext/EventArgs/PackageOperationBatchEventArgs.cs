// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageOperationBatchEventArgs.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System.ComponentModel;
    using Catel;
    using NuGet.Protocol.Core.Types;

    public class PackageOperationBatchEventArgs : CancelEventArgs
    {
        #region Constructors
        internal PackageOperationBatchEventArgs(PackageOperationType operationType, params IPackageSearchMetadata[] packages)
        {
            Argument.IsNotNull(() => packages);

            Packages = packages;
            OperationType = operationType;
        }
        #endregion

        #region Properties
        public IPackageSearchMetadata[] Packages { get; private set; }
        public PackageOperationType OperationType { get; private set; }
        #endregion
    }
}
