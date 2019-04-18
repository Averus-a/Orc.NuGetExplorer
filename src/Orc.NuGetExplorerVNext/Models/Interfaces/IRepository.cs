// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRepository.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using NuGet.Protocol.Core.Types;

    public interface IRepository
    {
        #region Properties
        string Name { get; }
        string Source { get; }
        PackageOperationType OperationType { get; }
        SourceRepository SourceRepository { get; }
        #endregion
    }
}
