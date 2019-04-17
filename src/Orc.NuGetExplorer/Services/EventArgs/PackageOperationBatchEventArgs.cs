// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageOperationBatchEventArgs.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System.ComponentModel;
    using Catel;

    public class PackageOperationBatchEventArgs : CancelEventArgs
    {
        #region Constructors
        internal PackageOperationBatchEventArgs(PackageOperationType operationType, params IPackage[] packages)
        {
            Argument.IsNotNullOrEmptyArray(() => packages);

            Packages = packages;
            OperationType = operationType;
        }
        #endregion

        #region Properties
        public IPackage[] Packages { get; private set; }
        public PackageOperationType OperationType { get; private set; }
        #endregion
    }
}