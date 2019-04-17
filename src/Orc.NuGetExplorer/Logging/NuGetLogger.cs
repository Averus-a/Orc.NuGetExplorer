// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NuGetLogger.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System.Threading.Tasks;
    using Catel;
    using Catel.Threading;
    using NuGet.Common;

    internal class NuGetLogger : LoggerBase
    {
        #region Fields
        private readonly INuGetLogListeningSevice _logListeningService;
        #endregion

        #region Constructors
        public NuGetLogger(INuGetLogListeningSevice logListeningService)
        {
            Argument.IsNotNull(() => logListeningService);

            _logListeningService = logListeningService;
        }
        #endregion
        
        #region Methods
        public override void Log(ILogMessage message)
        {
            switch (message.Level)
            {
                case LogLevel.Debug:
                    _logListeningService.SendDebug(message.Message);
                    break;

                case LogLevel.Information:
                case LogLevel.Verbose:
                case LogLevel.Minimal:
                    _logListeningService.SendInfo(message.Message);
                    break;

                case LogLevel.Error:
                    _logListeningService.SendError(message.Message);
                    break;

                case LogLevel.Warning:
                    _logListeningService.SendWarning(message.Message);
                    break;
            }
        }

        public override Task LogAsync(ILogMessage message)
        {
            Log(message);

            return TaskHelper.Completed;
        }
        #endregion
    }
}
