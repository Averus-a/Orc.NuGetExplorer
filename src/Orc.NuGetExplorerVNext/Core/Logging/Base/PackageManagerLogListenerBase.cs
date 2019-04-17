// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageManagerLogListenerBase.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using Catel;

    public abstract class PackageManagerLogListenerBase
    {
        #region Constructors
        protected PackageManagerLogListenerBase(INuGetLogListeningService nuGetLogListeningService)
        {
            Argument.IsNotNull(() => nuGetLogListeningService);

            nuGetLogListeningService.Error += OnError;
            nuGetLogListeningService.Info += OnInfo;
            nuGetLogListeningService.Debug += OnDebug;
            nuGetLogListeningService.Warning += OnWarning;
        }
        #endregion

        #region Methods
        protected virtual void OnWarning(object sender, NuGetLogRecordEventArgs e)
        {
        }

        protected virtual void OnDebug(object sender, NuGetLogRecordEventArgs e)
        {
        }

        protected virtual void OnInfo(object sender, NuGetLogRecordEventArgs e)
        {
        }

        protected virtual void OnError(object sender, NuGetLogRecordEventArgs e)
        {
        }
        #endregion
    }
}
