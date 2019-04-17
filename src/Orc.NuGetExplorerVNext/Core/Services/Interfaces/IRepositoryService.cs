// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRepositoryService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System.Collections.Generic;
    using NuGet.Protocol.Core.Types;

    public interface IRepositoryService
    {
        #region Properties
        SourceRepository LocalRepository { get; }
        #endregion

        #region Methods
        IEnumerable<IRepository> GetRepositories(PackageOperationType packageOperationType);
        IEnumerable<IRepository> GetSourceRepositories();
        IRepository GetSourceAggregateRepository();
        IEnumerable<IRepository> GetUpdateRepositories();
        IRepository GetUpdateAggeregateRepository();
        #endregion
    }
}
