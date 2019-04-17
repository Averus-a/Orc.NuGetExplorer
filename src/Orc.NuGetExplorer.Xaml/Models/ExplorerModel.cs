// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExplorerModel.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System.Threading;
    using Catel.Collections;
    using Catel.Data;

    internal class ExplorerModel : ModelBase
    {
        #region Constructors
        public ExplorerModel()
        {
            PackageList = new FastObservableCollection<IPackage>();

            Stop();
        }
        #endregion

        #region Properties
        public SearchCursor SearchCursor;
        private CancellationTokenSource _cancellationTokenSource;
        public FastObservableCollection<IPackage> PackageList { get; set; }

        public CancellationToken CancellationToken
        {
            get
            {
                if (_cancellationTokenSource is null)
                {
                    return CancellationToken.None;
                }

                return _cancellationTokenSource.Token;
            }
        } 
        #endregion

        public void Stop()
        {
            if (!(_cancellationTokenSource is null))
            {
                _cancellationTokenSource.Cancel(false);
            
                _cancellationTokenSource.Dispose();
            }
            
            _cancellationTokenSource = new CancellationTokenSource();
        }

    }
}
