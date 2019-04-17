// --------------------------------------------------------------------------------------------------------------------
// <copyright file="INuGetProjectProvider.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using NuGet.ProjectManagement;

    public interface INuGetProjectProvider
    {
        NuGetProject GetProject();
    }
}
