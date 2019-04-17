// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectContext.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using System;
    using System.Xml.Linq;
    using NuGet.Packaging;
    using NuGet.ProjectManagement;

    public class ProjectContext : INuGetProjectContext
    {
        public void Log(MessageLevel level, string message, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void ReportError(string message)
        {
            throw new NotImplementedException();
        }

        public FileConflictAction ResolveFileConflict(string message)
        {
            throw new NotImplementedException();
        }

        public PackageExtractionContext PackageExtractionContext { get; set; }
        public ISourceControlManagerProvider SourceControlManagerProvider { get; }
        public ExecutionContext ExecutionContext { get; }
        public XDocument OriginalPackagesConfig { get; set; }
        public NuGetActionType ActionType { get; set; }
        public Guid OperationId { get; set; }
    }
}
