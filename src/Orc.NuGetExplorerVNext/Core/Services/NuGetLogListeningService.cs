// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NuGetLogListeningService.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using System;
    using Catel;

    public class NuGetLogListeningService : INuGetLogListeningService
    {
        #region Methods
        public void SendInfo(string message)
        {
            Argument.IsNotNullOrEmpty(() => message);

            Info?.Invoke(this, new NuGetLogRecordEventArgs(message));
        }

        public void SendWarning(string message)
        {
            Argument.IsNotNullOrEmpty(() => message);

            Warning?.Invoke(this, new NuGetLogRecordEventArgs(message));
        }

        public void SendDebug(string message)
        {
            Argument.IsNotNullOrEmpty(() => message);

            Debug?.Invoke(this, new NuGetLogRecordEventArgs(message));
        }

        public void SendError(string message)
        {
            Argument.IsNotNullOrEmpty(() => message);

            Error?.Invoke(this, new NuGetLogRecordEventArgs(message));
        }

        public event EventHandler<NuGetLogRecordEventArgs> Info;
        public event EventHandler<NuGetLogRecordEventArgs> Warning;
        public event EventHandler<NuGetLogRecordEventArgs> Debug;
        public event EventHandler<NuGetLogRecordEventArgs> Error;
        #endregion
    }
}
