// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IApiPackageRegistry.cs" company="WildGums">
//   Copyright (c) 2008 - 2017 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using System.Threading.Tasks;

    public interface IApiPackageRegistry
    {
        void Register(string packageName, string version);

        bool IsRegistered(string packageName);

        Task ValidateAsync(IPackageDetails package);
    }
}
