// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RepositoryService.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Catel;
    using NuGet.Protocol.Core.Types;

    public class RepositoryService : IRepositoryService
    {
        private readonly ISourceRepositoryProvider _sourceRepositoryProvider;

        public RepositoryService(ISourceRepositoryProvider sourceRepositoryProvider)
        {
            Argument.IsNotNull(() => sourceRepositoryProvider);

            _sourceRepositoryProvider = sourceRepositoryProvider;
        }

        public SourceRepository LocalRepository { get; }

        public IEnumerable<IRepository> GetRepositories(PackageOperationType packageOperationType)
        {
            var sourceRepositories = _sourceRepositoryProvider.GetRepositories();

            return sourceRepositories.Select(x => new Repository(x));
        }

        public IEnumerable<IRepository> GetSourceRepositories()
        {
            throw new System.NotImplementedException();
        }

        public IRepository GetSourceAggregateRepository()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<IRepository> GetUpdateRepositories()
        {
            throw new System.NotImplementedException();
        }

        public IRepository GetUpdateAggeregateRepository()
        {
            throw new System.NotImplementedException();
        }
    }
}
