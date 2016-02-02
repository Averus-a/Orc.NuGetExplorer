﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RepositoryCacheService.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System;
    using System.Collections.Generic;
    using Catel;
    using Catel.Logging;
    using NuGet;

    internal class RepositoryCacheService : IRepositoryCacheService
    {
        #region Fields
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private static int _idCounter;
        private readonly IDictionary<int, Tuple<IRepository, IPackageRepository>> _idTupleDictionary = new Dictionary<int, Tuple<IRepository, IPackageRepository>>();
        private readonly IDictionary<string, int> _keyIdDictionary = new Dictionary<string, int>();
        #endregion

        #region Methods
        public IRepository GetSerializableRepository(string name, string source, PackageOperationType operationType, Func<IPackageRepository> packageRepositoryFactory, bool renew = false)
        {
            Argument.IsNotNullOrEmpty(() => name);
            Argument.IsNotNull(() => packageRepositoryFactory);

            var key = GetKey(operationType, name);

            int id;
            if (_keyIdDictionary.TryGetValue(key, out id))
            {
                if (!renew)
                {
                    return _idTupleDictionary[id].Item1;
                }

                return CreateSerializableRepository(id, name, source, operationType, packageRepositoryFactory);
            }

            id = _idCounter++;
            _keyIdDictionary.Add(key, id);

            return CreateSerializableRepository(id, name, source, operationType, packageRepositoryFactory);
        }

        [ObsoleteEx(ReplacementTypeOrMember = "GetSerializableRepository", TreatAsErrorFromVersion = "1.1", RemoveInVersion = "2.0")]
        public IRepository GetSerialisableRepository(string name, PackageOperationType operationType, Func<IPackageRepository> packageRepositoryFactory, bool renew = false)
        {
            return GetSerializableRepository(name, string.Empty, operationType, packageRepositoryFactory, renew);
        }

        private IRepository CreateSerializableRepository(int id, string name, string source, PackageOperationType operationType, Func<IPackageRepository> packageRepositoryFactory)
        {
            Argument.IsNotNullOrEmpty(() => name);
            Argument.IsNotNullOrEmpty(() => source);
            Argument.IsNotNull(() => packageRepositoryFactory);

            var repository = new Repository
            {
                Id = id,
                Name = name,
                Source = source,
                OperationType = operationType
            };
            
            _idTupleDictionary[id] = new Tuple<IRepository, IPackageRepository>(repository, packageRepositoryFactory());

            return repository;
        }

        public IPackageRepository GetNuGetRepository(IRepository repository)
        {
            Argument.IsNotNull(() => repository);

            var id = ((Repository) repository).Id;

            return _idTupleDictionary[id].Item2;
        }

        private static string GetKey(PackageOperationType operationType, string name)
        {
            return string.Format("{0}_{1}", operationType, name);
        }
        #endregion
    }
}