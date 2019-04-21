// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPackageCommandService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System.Threading.Tasks;
    using NuGet.Protocol.Core.Types;

    public interface IPackageCommandService
    {
        #region Methods
        string GetActionName(PackageOperationType operationType);
        Task ExecuteAsync(PackageOperationType operationType, IPackageSearchMetadata package, IRepository sourceRepository = null, bool allowedPrerelease = false);
        Task<bool> CanExecuteAsync(PackageOperationType operationType, IPackageSearchMetadata package);
        bool IsRefreshRequired(PackageOperationType operationType);
        string GetPluralActionName(PackageOperationType operationType);
        #endregion
    }
}
