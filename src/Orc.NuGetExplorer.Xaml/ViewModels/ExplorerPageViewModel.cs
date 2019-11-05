﻿namespace Orc.NuGetExplorer.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using Catel;
    using Catel.Collections;
    using Catel.Data;
    using Catel.IoC;
    using Catel.Logging;
    using Catel.MVVM;
    using Catel.Services;
    using Catel.Windows.Threading;
    using Enums;
    using Management;
    using Management.EventArgs;
    using Models;
    using NuGet.Configuration;
    using NuGet.Protocol.Core.Types;
    using Pagination;
    using Services;
    using Web;
    using Timer = System.Timers.Timer;

    internal class ExplorerPageViewModel : ViewModelBase, IManagerPage
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private static readonly int PageSize = 17;
        private static readonly int SingleTasksDelayMs = 800;
        private static readonly IHttpExceptionHandler<FatalProtocolException> PackageLoadingExceptionHandler = new FatalProtocolExceptionHandler();

        private static readonly Timer SingleDelayTimer = new Timer(SingleTasksDelayMs);

#pragma warning disable IDE1006 // Naming Styles
        private static IDisposable _context;
#pragma warning restore IDE1006 // Naming Styles
        private readonly IDefferedPackageLoaderService _defferedPackageLoaderService;
        private readonly IDispatcherService _dispatcherService;
        private readonly INuGetFeedVerificationService _nuGetFeedVerificationService;
        private readonly IPackageMetadataMediaDownloadService _packageMetadataMediaDownloadService;


        private readonly IPackagesLoaderService _packagesLoaderService;

        private readonly MetadataOrigin _pageType;
        private readonly INuGetPackageManager _projectManager;
        private readonly IRepositoryContextService _repositoryService;

        private readonly HashSet<CancellationTokenSource> _tokenSource = new HashSet<CancellationTokenSource>();
        private readonly ITypeFactory _typeFactory;

        private ExplorerSettingsContainer _settings;

        public ExplorerPageViewModel(ExplorerSettingsContainer explorerSettings, string pageTitle, IPackagesLoaderService packagesLoaderService,
            IPackageMetadataMediaDownloadService packageMetadataMediaDownloadService, INuGetFeedVerificationService nuGetFeedVerificationService,
            ICommandManager commandManager, IDispatcherService dispatcherService, IRepositoryContextService repositoryService, ITypeFactory typeFactory,
            IDefferedPackageLoaderService defferedPackageLoaderService, INuGetPackageManager projectManager)
        {
            Title = pageTitle;

            Argument.IsNotNull(() => packagesLoaderService);
            Argument.IsNotNull(() => explorerSettings);
            Argument.IsNotNull(() => packageMetadataMediaDownloadService);
            Argument.IsNotNull(() => commandManager);
            Argument.IsNotNull(() => nuGetFeedVerificationService);
            Argument.IsNotNull(() => dispatcherService);
            Argument.IsNotNull(() => repositoryService);
            Argument.IsNotNull(() => typeFactory);
            Argument.IsNotNull(() => defferedPackageLoaderService);
            Argument.IsNotNull(() => projectManager);

            _packagesLoaderService = packagesLoaderService;

            if (Title != "Browse")
            {
                _packagesLoaderService = this.GetServiceLocator().ResolveType<IPackagesLoaderService>(Title);
            }


            if (!Enum.TryParse(Title, out _pageType))
            {
                Log.Error("Unrecognized page type");
            }

            CanBatchProjectActions = _pageType == MetadataOrigin.Updates;

            _dispatcherService = dispatcherService;
            _packageMetadataMediaDownloadService = packageMetadataMediaDownloadService;
            _nuGetFeedVerificationService = nuGetFeedVerificationService;
            _repositoryService = repositoryService;
            _defferedPackageLoaderService = defferedPackageLoaderService;
            _projectManager = projectManager;

            Settings = explorerSettings;

            _typeFactory = typeFactory;

            LoadNextPackagePage = new TaskCommand(LoadNextPackagePageExecute);
            CancelPageLoading = new TaskCommand(CancelPageLoadingExecute);
            RefreshCurrentPage = new TaskCommand(RefreshCurrentPageExecute);

            commandManager.RegisterCommand(nameof(RefreshCurrentPage), RefreshCurrentPage, this);
        }

        /// <summary>
        ///     Repository context.
        ///     Due to all pages uses package sources selected by user in settings
        ///     context is shared between pages too
        /// </summary>
        private static IDisposable Context
        {
            get { return _context; }
            set
            {
                if (_context != value)
                {
                    _context?.Dispose();
                    _context = value;
                }
            }
        }


        public static CancellationTokenSource VerificationTokenSource { get; set; } = new CancellationTokenSource();

        public static CancellationTokenSource DelayCancellationTokenSource { get; set; } = new CancellationTokenSource();

        private PageContinuation PageInfo { get; set; }

        private PageContinuation AwaitedPageInfo { get; set; }

        private PackageSearchParameters AwaitedSearchParameters { get; set; }

        public ExplorerSettingsContainer Settings
        {
            get { return _settings; }
            set
            {
                if (_settings != null)
                {
                    _settings.PropertyChanged -= OnSettingsPropertyPropertyChanged;
                }

                _settings = value;

                if (_settings != null)
                {
                    _settings.PropertyChanged += OnSettingsPropertyPropertyChanged;
                }
            }
        }


        public NuGetPackage SelectedPackageItem { get; set; }

        public bool IsActive { get; set; }

        /// <summary>
        ///     Shows is data should be reloaded
        ///     when viewmodel became active
        /// </summary>
        public bool Invalidated { get; set; }

        public bool IsCancellationTokenAlive { get; set; }

        public bool IsLoadingInProcess { get; set; }

        public bool IsFirstLoaded { get; set; } = true;

        public bool IsCancellationForced { get; set; }

        /// <summary>
        ///     Is project manipulations can be performed on multiple packages
        ///     on this page in one operation
        /// </summary>
        public bool CanBatchProjectActions { get; set; }

        public CancellationTokenSource PageLoadingTokenSource { get; set; }

        public FastObservableCollection<NuGetPackage> PackageItems { get; set; }

        public void StartLoadingTimerOrInvalidateData()
        {
            if (IsActive)
            {
                StartLoadingTimer();
            }
            else
            {
                Invalidated = true;
            }
        }

        private async void OnSettingsPropertyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Settings.ObservedFeed == null)
            {
                return;
            }

            if (IsFirstLoaded)
            {
                return;
            }

            if (string.Equals(e.PropertyName, nameof(Settings.IsPreReleaseIncluded)) ||
                string.Equals(e.PropertyName, nameof(Settings.SearchString)) || string.Equals(e.PropertyName, nameof(Settings.ObservedFeed)))
            {
                StartLoadingTimerOrInvalidateData();
            }
        }

        private void OnPackageItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //item added on first place, collection was empty
                if (e.NewStartingIndex == 0 && IsActive)
                {
                    SelectedPackageItem = PackageItems.FirstOrDefault();
                }
            }
        }

        protected override async Task InitializeAsync()
        {
            try
            {
                //execution delay
                SingleDelayTimer.Elapsed += OnTimerElapsed;
                SingleDelayTimer.AutoReset = false;

                SingleDelayTimer.SynchronizingObject = _typeFactory.CreateInstanceWithParameters<ISynchronizeInvoke>(DispatcherHelper.CurrentDispatcher);

                PackageItems = new FastObservableCollection<NuGetPackage>();

                PackageItems.CollectionChanged += OnPackageItemsCollectionChanged;

                _projectManager.Install += OnProjectManagerInstall;
                _projectManager.Uninstall += OnProjectManagerUninstall;
                _projectManager.Update += OnProjectManagerUpdate;

                IsFirstLoaded = false;

                //todo validation
                if (Settings.ObservedFeed != null && !string.IsNullOrEmpty(Settings.ObservedFeed.Source))
                {
                    var currentFeed = Settings.ObservedFeed;
                    PageInfo = new PageContinuation(PageSize, Settings.ObservedFeed.GetPackageSource());
                    var searchParams = new PackageSearchParameters(Settings.IsPreReleaseIncluded, Settings.SearchString);

                    await VerifySourceAndLoadPackagesAsync(PageInfo, currentFeed, searchParams);
                }
                else
                {
                    Log.Info("None of the source feeds configured");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (string.Equals(e.PropertyName, nameof(Invalidated)))
            {
                Log.Info($"ViewModel {this} {e.PropertyName} flag set to {Invalidated}");
            }

            if (string.Equals(e.PropertyName, nameof(IsActive)))
            {
                if ((bool)e.NewValue)
                {
                    Log.Info($"Switched page: {Title} is active");

                    //force update selected item
                    SelectedPackageItem = PackageItems?.FirstOrDefault();
                }
            }

            if (IsFirstLoaded)
            {
                return;
            }

            if (string.Equals(e.PropertyName, nameof(IsActive)) && Invalidated)
            {
                //just switching page, no need to invalidate data
                StartLoadingTimer();
            }
        }

        protected override async Task OnClosedAsync(bool? result)
        {
            PackageItems.CollectionChanged -= OnPackageItemsCollectionChanged;
        }

        private void StartLoadingTimer()
        {
            if (SingleDelayTimer.Enabled)
            {
                SingleDelayTimer.Stop();
            }

            SingleDelayTimer.Start();

            Log.Debug("Start loading delay timer");
        }

        private async void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Log.Info("Timer elapsed");
            var currentFeed = Settings.ObservedFeed;
            //reset page
            PageInfo = new PageContinuation(PageSize, currentFeed.GetPackageSource());

            var searchParams = new PackageSearchParameters(Settings.IsPreReleaseIncluded, Settings.SearchString);
            await VerifySourceAndLoadPackagesAsync(PageInfo, currentFeed, searchParams);
        }

        private async Task VerifySourceAndLoadPackagesAsync(PageContinuation pageinfo, INuGetSource currentSource, PackageSearchParameters searchParams)
        {
            try
            {
                if (pageinfo.Source.IsMultipleSource)
                {
                    Context = _repositoryService.AcquireContext();
                }
                else
                {
                    Context = _repositoryService.AcquireContext((PackageSource)pageinfo.Source);
                }


                if (IsActive)
                {
                    IsCancellationTokenAlive = true;
                    Log.Info("You can now cancel search from gui");

                    using (var pageTcs = GetCancelationTokenSource())
                    {
                        if (!currentSource.IsVerified)
                        {
                            await CanFeedBeLoadedAsync(VerificationTokenSource.Token, currentSource);
                        }

                        if (!currentSource.IsAccessible)
                        {
                            IsCancellationTokenAlive = false;
                            return;
                        }

                        if (!IsLoadingInProcess)
                        {
                            await LoadPackagesAsync(pageinfo, pageTcs.Token, searchParams);
                        }
                        else
                        {
                            if (IsCancellationForced)
                            {
                                //to prevent load restarting if cancellation initiated by user
                                AwaitedPageInfo = null;
                            }
                            else
                            {
                                AwaitedPageInfo = PageInfo;
                                AwaitedSearchParameters = searchParams;
                            }

                            //task with pageTcs source cancel all loading tasks in-process
                            CancelLoadingTasks(pageTcs);
                        }

                        _tokenSource.Remove(pageTcs);

                        PageLoadingTokenSource = null;
                        IsCancellationTokenAlive = false;
                    }
                }
            }
            catch (OperationCanceledException e)
            {
                Log.Info($"Command {nameof(LoadPackagesAsync)} was cancelled by {e}");

                IsCancellationTokenAlive = false;

                //backward page if needed
                if (PageInfo.LastNumber > PageSize)
                {
                    PageInfo.GetPrevious();
                }

                //restart
                if (AwaitedPageInfo != null)
                {
                    var awaitedPageinfo = AwaitedPageInfo;
                    var awaitedSeachParams = AwaitedSearchParameters;
                    AwaitedPageInfo = null;
                    AwaitedSearchParameters = null;
                    await VerifySourceAndLoadPackagesAsync(awaitedPageinfo, Settings.ObservedFeed, awaitedSeachParams);
                }
                else
                {
                    Log.Info("Search operation was canceled (interrupted by next user request");
                }
            }
            catch (FatalProtocolException ex)
            {
                IsCancellationTokenAlive = false;
                var result = PackageLoadingExceptionHandler.HandleException(ex, currentSource.Source);

                if (result == FeedVerificationResult.AuthenticationRequired)
                {
                    Log.Error($"Authentication credentials required. Cannot load packages from source '{currentSource.Source}'");
                }
                else
                {
                    Log.Error(ex);
                }
            }
            catch (Exception ex)
            {
                IsCancellationTokenAlive = false;
                Log.Error(ex);
            }
            finally
            {
                await _defferedPackageLoaderService.StartLoadingAsync();
            }
        }

        private CancellationTokenSource GetCancelationTokenSource()
        {
            var source = new CancellationTokenSource();

            _tokenSource.Add(source);

            return source;
        }

        private void CancelLoadingTasks(CancellationTokenSource token)
        {
            foreach (var tokenSource in _tokenSource)
            {
                if (tokenSource != token)
                {
                    tokenSource.Cancel();
                }
            }
        }

        private async Task LoadPackagesAsync(PageContinuation pageInfo, CancellationToken cancellationToken, PackageSearchParameters searchParameters)
        {
            try
            {
                IsLoadingInProcess = true;

                Log.Info($"Start query {Title} page");

                var isFirstLoad = pageInfo.Current < 0;

                if (isFirstLoad)
                {
                    PackageItems.Clear();
                }

                var packages = await _packagesLoaderService.LoadAsync(
                    searchParameters.SearchString, pageInfo, new SearchFilter(searchParameters.IsPrereleaseIncluded), cancellationToken);

                await DownloadAllPicturesForMetadataAsync(packages, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                await CreatePackageListItems(packages);

                Invalidated = false;

                Log.Info($"Page {Title} updates with {packages.Count()} returned by query '{Settings.SearchString} from {PageInfo.Source}'");
            }
            finally
            {
                IsLoadingInProcess = false;
            }
        }

        private async Task CreatePackageListItems(IEnumerable<IPackageSearchMetadata> packageSearchMetadataCollection)
        {
            var vms = packageSearchMetadataCollection.Select(x => _typeFactory.CreateInstanceWithParametersAndAutoCompletion<NuGetPackage>(x, _pageType)).ToList();

            //create tokens, used for deffer execution of tasks
            //obtained states/updates of packages

            if (_pageType != MetadataOrigin.Updates)
            {
                foreach (var vm in vms)
                {
                    var deferToken = new DeferToken();

                    deferToken.LoadType = DetermineLoadBehavior(_pageType);
                    deferToken.Package = vm;

                    deferToken.UpdateAction = newState =>
                    {
                        vm.Status = newState;
                    };

                    _defferedPackageLoaderService.Add(deferToken);
                }
            }

            _dispatcherService.BeginInvoke(() =>
            {
                PackageItems.AddRange(vms);
            }
            );

            MetadataOrigin DetermineLoadBehavior(MetadataOrigin page)
            {
                switch (page)
                {
                    case MetadataOrigin.Browse: return MetadataOrigin.Installed;

                    case MetadataOrigin.Installed: return MetadataOrigin.Browse;
                }

                return MetadataOrigin.Browse;
            }
        }

        private async Task DownloadAllPicturesForMetadataAsync(IEnumerable<IPackageSearchMetadata> metadatas, CancellationToken token)
        {
            foreach (var metadata in metadatas)
            {
                if (metadata.IconUrl != null)
                {
                    token.ThrowIfCancellationRequested();
                    await _packageMetadataMediaDownloadService.DownloadFromAsync(metadata);
                }
            }

            await Task.CompletedTask;
        }

        private async Task OnProjectManagerUninstall(object sender, UninstallNuGetProjectEventArgs e)
        {
            var batchedArgs = e as BatchedUninstallNuGetProjectEventArgs;

            if (!batchedArgs.IsBatchEnd)
            {
                return;
            }

            StartLoadingTimerOrInvalidateData();
        }

        private async Task OnProjectManagerInstall(object sender, InstallNuGetProjectEventArgs e)
        {
            var batchedArgs = e as BatchedInstallNuGetProjectEventArgs;

            if (!batchedArgs.IsBatchEnd)
            {
                return;
            }

            StartLoadingTimerOrInvalidateData();
        }

        private async Task OnProjectManagerUpdate(object sender, UpdateNuGetProjectEventArgs e)
        {
            StartLoadingTimerOrInvalidateData();
        }

        private async Task CanFeedBeLoadedAsync(CancellationToken cancelToken, INuGetSource source)
        {
            Log.Info($"{source} is verified");

            if (source is NuGetFeed)
            {
                var singleSource = source as NuGetFeed;

                singleSource.VerificationResult = singleSource.IsLocal()
                    ? FeedVerificationResult.Valid
                    : await _nuGetFeedVerificationService.VerifyFeedAsync(source.Source, cancelToken);
            }
            else if (source is CombinedNuGetSource)
            {
                var combinedSource = source as CombinedNuGetSource;
                var unaccessibleFeeds = new List<NuGetFeed>();

                foreach (var feed in combinedSource.GetAllSources())
                {
                    feed.VerificationResult = feed.IsLocal()
                        ? FeedVerificationResult.Valid
                        : await _nuGetFeedVerificationService.VerifyFeedAsync(feed.Source, cancelToken);

                    if (!feed.IsAccessible)
                    {
                        unaccessibleFeeds.Add(feed);
                        Log.Warning($"{feed} is unaccessible. It won't be used when 'All' option selected");
                    }
                }

                unaccessibleFeeds.ForEach(x => combinedSource.RemoveFeed(x));
            }
            else
            {
                Log.Error($"Parameter {source} has invalid type");
            }
        }

        #region commands
        public TaskCommand LoadNextPackagePage { get; set; }

        private async Task LoadNextPackagePageExecute()
        {
            var pageInfo = PageInfo;
            var searchParams = new PackageSearchParameters(Settings.IsPreReleaseIncluded, Settings.SearchString);
            await VerifySourceAndLoadPackagesAsync(pageInfo, Settings.ObservedFeed, searchParams);
        }

        public TaskCommand CancelPageLoading { get; set; }

        private async Task CancelPageLoadingExecute()
        {
            IsCancellationForced = true;

            //force cancel all operations
            if (IsCancellationTokenAlive)
            {
                foreach (var token in _tokenSource)
                {
                    token.Cancel();
                }
            }

            IsCancellationForced = false;
        }

        public TaskCommand RefreshCurrentPage { get; set; }

        private async Task RefreshCurrentPageExecute()
        {
            StartLoadingTimerOrInvalidateData();
        }
        #endregion
    }
}
