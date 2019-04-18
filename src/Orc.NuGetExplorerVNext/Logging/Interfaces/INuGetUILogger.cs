// --------------------------------------------------------------------------------------------------------------------
// <copyright file="INuGetUILogger.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using NuGet.Common;
    using NuGet.ProjectManagement;

    public interface INuGetUILogger
    {
        void Log(MessageLevel level, string message, params object[] args);

        void ReportError(string message);

        void ReportError(ILogMessage message);

        void Start();

        void End();
    }
}
