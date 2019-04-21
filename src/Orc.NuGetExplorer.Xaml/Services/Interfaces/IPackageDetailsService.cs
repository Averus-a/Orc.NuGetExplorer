// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPackageDetailsService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System.Threading.Tasks;
    using System.Windows.Documents;
    using NuGet.Protocol.Core.Types;

    internal interface IPackageDetailsService
    {
        #region Methods
        Task<FlowDocument> PackageToFlowDocumentAsync(IPackageSearchMetadata package);
        #endregion
    }
}
