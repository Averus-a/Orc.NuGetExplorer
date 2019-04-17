// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NuGetProjectProvider.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using NuGet.ProjectManagement;

    public class NuGetProjectProvider : INuGetProjectProvider
    {
        public NuGetProject GetProject()
        {
            return new FolderNuGetProject(_packageFolderPath)
        }
    }
}
