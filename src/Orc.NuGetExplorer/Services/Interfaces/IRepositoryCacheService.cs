// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRepositoryCacheService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System;
    using NuGet;
    using NuGet.Protocol.Core.Types;

    internal interface IRepositoryCacheService
    {
        #region Methods
        IRepository GetSerializableRepository(string name, string source, PackageOperationType operationType, Func<SourceRepository> packageRepositoryFactory, bool renew = false);
        SourceRepository GetNuGetRepository(IRepository repository);
        #endregion
    }
}
