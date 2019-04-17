// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExplorerViewModel.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Media.Animation;
    using Catel;
    using Catel.Configuration;
    using Catel.Fody;
    using Catel.Logging;
    using Catel.MVVM;
    using Catel.Scoping;
    using Catel.Services;
    using Catel.Threading;
    using MethodTimer;
    using NuGet;
    using Scopes;
    using CollectionExtensions = Catel.Collections.CollectionExtensions;

    internal class ExplorerViewModel : ViewModelBase
    {
        #region Fields
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        
        private readonly IDispatcherService _dispatcherService;
        private readonly IPackageBatchService _packageBatchService;
        private readonly INuGetConfigurationService _nuGetConfigurationService;
        private readonly IConfigurationService _configurationService;
        private readonly IRepositoryNavigatorService _repositoryNavigatorService;
        private readonly IPackageCommandService _packageCommandService;
        private readonly IPackageQueryService _packageQueryService;
        private readonly IPackagesUpdatesSearcherService _packagesUpdatesSearcherService;
        private readonly IPleaseWaitService _pleaseWaitService;

        private bool _searchingAndRefreshing;
        #endregion

        #region Constructors
        public ExplorerViewModel(IRepositoryNavigatorService repositoryNavigatorService, ISearchSettingsService searchSettingsService, IPackageCommandService packageCommandService,
            IPleaseWaitService pleaseWaitService, IPackageQueryService packageQueryService, IDispatcherService dispatcherService,
            IPackagesUpdatesSearcherService packagesUpdatesSearcherService, IPackageBatchService packageBatchService, INuGetConfigurationService nuGetConfigurationService,
            IConfigurationService configurationService)
        {
            Argument.IsNotNull(() => repositoryNavigatorService);
            Argument.IsNotNull(() => searchSettingsService);
            Argument.IsNotNull(() => packageCommandService);
            Argument.IsNotNull(() => pleaseWaitService);
            Argument.IsNotNull(() => packageQueryService);
            Argument.IsNotNull(() => dispatcherService);
            Argument.IsNotNull(() => packagesUpdatesSearcherService);
            Argument.IsNotNull(() => packageBatchService);
            Argument.IsNotNull(() => nuGetConfigurationService);
            Argument.IsNotNull(() => configurationService);

            _repositoryNavigatorService = repositoryNavigatorService;
            _packageCommandService = packageCommandService;
            _pleaseWaitService = pleaseWaitService;
            _packageQueryService = packageQueryService;
            _dispatcherService = dispatcherService;
            _packagesUpdatesSearcherService = packagesUpdatesSearcherService;
            _packageBatchService = packageBatchService;
            _nuGetConfigurationService = nuGetConfigurationService;
            _configurationService = configurationService;

            SearchSettings = searchSettingsService.SearchSettings;

            AvailableUpdates = new ObservableCollection<IPackageDetails>();

            PackageAction = new TaskCommand<IPackageDetails>(OnPackageActionExecuteAsync, OnPackageActionCanExecute);
            CheckForUpdates = new TaskCommand(OnCheckForUpdatesExecuteAsync);
            OpenUpdateWindow = new TaskCommand(OnOpenUpdateWindowExecuteAsync);

            AccentColorHelper.CreateAccentColorResourceDictionary();
        }
        #endregion

        #region Properties
        [Model]
        [Expose(nameof(RepositoryNavigator.RepositoryCategories))]
        [Expose(nameof(RepositoryNavigator.SelectedRepository))]
        public RepositoryNavigator Navigator { get; private set; }

        [Model]
        [Expose(nameof(NuGetExplorer.SearchSettings.SearchFilter))]
        [Expose(nameof(NuGetExplorer.SearchSettings.PackagesToSkip))]
        public SearchSettings SearchSettings { get; private set; }

        [ViewModelToModel(nameof(SearchSettings))]
        public bool? IsPrereleaseAllowed { get; set; }

        //[Model]
        //[Expose(nameof(NuGetExplorer.SearchResult.CanContinue))]
        
        ////TODO: this is causing build error !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        ////[Expose(nameof(NuGetExplorer.SearchResult.PackageList))]
        public SearchResult SearchResult { get; private set; }

        public string ActionName { get; private set; }
        public string FilterWatermark { get; private set; }
        public bool ShowUpdates { get; private set; }
        public IPackageDetails SelectedPackage { get; set; }
        public ObservableCollection<IPackageDetails> AvailableUpdates { get; private set; }
        #endregion

        #region Methods
        protected override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            _repositoryNavigatorService.Initialize();

            Navigator = _repositoryNavigatorService.Navigator;

            if (!Navigator.Initialized)
            {
                Navigator.Initialize();
            }

            await SearchAndRefreshAsync();
        }

        private void OnIsPrereleaseAllowedChanged()
        {
            if (!_searchingAndRefreshing && IsPrereleaseAllowed != null && Navigator.SelectedRepository != null)
            {
                _nuGetConfigurationService.SetIsPrereleaseAllowed(Navigator.SelectedRepository, IsPrereleaseAllowed.Value);
            }

#pragma warning disable 4014
            SearchAndRefreshAsync();
#pragma warning restore 4014
        }

        private void OnPackagesToSkipChanged()
        {
#pragma warning disable 4014
            SearchAndRefreshAsync();
#pragma warning restore 4014
        }

        private async Task SearchAndRefreshAsync()
        {
            if (_searchingAndRefreshing || SearchResult.PackageList == null || Navigator.SelectedRepository == null)
            {
                return;
            }

            using (new DisposableToken(this, x => _searchingAndRefreshing = true, x => _searchingAndRefreshing = false))
            {
                SetFilterWatermark();
                SetShowUpdates();
                SetActionName();
                SetIsPrereleaseAllowed();
                await CountAndSearchAsync();
                await RefreshCanExecuteAsync();
            }
        }

        private void SetIsPrereleaseAllowed()
        {
            switch (Navigator.SelectedRepository.OperationType)
            {
                case PackageOperationType.Install:
                case PackageOperationType.Update:
                    IsPrereleaseAllowed = _nuGetConfigurationService.GetIsPrereleaseAllowed(Navigator.SelectedRepository);
                    break;

                default:
                    IsPrereleaseAllowed = null;
                    break;
            }
        }

        private void SetActionName()
        {
            ActionName = _packageCommandService.GetActionName(Navigator.SelectedRepository.OperationType);
        }

        private void OnSelectedRepositoryChanged()
        {
            if (_searchingAndRefreshing || SearchResult.PackageList == null || Navigator.SelectedRepository == null)
            {
                return;
            }

            SearchSettings.PackagesToSkip = 0;

            var selectedRepository = Navigator.SelectedRepository;
            var selectedRepositoryCategory = Navigator.SelectedRepositoryCategory;

            if (selectedRepositoryCategory == null || selectedRepository == null)
            {
                return;
            }

            _configurationService.SetLastRepository(selectedRepositoryCategory, selectedRepository);

#pragma warning disable 4014
            SearchAndRefreshAsync();
#pragma warning restore 4014
        }

        private void SetShowUpdates()
        {
            switch (Navigator.SelectedRepository.OperationType)
            {
                case PackageOperationType.Uninstall:
                    ShowUpdates = false;
                    break;

                case PackageOperationType.Install:
                    ShowUpdates = false;
                    break;

                case PackageOperationType.Update:
                    ShowUpdates = true;
                    break;

                default:
                    ShowUpdates = false;
                    break;
            }
        }

        private void SetFilterWatermark()
        {
            const string defaultWatermark = "Search";

            if (Navigator.SelectedRepository == null)
            {
                FilterWatermark = defaultWatermark;
                return;
            }

            switch (Navigator.SelectedRepository.OperationType)
            {
                case PackageOperationType.Uninstall:
                    FilterWatermark = "Search in Installed";
                    break;

                case PackageOperationType.Install:
                    FilterWatermark = "Search Online";
                    break;

                case PackageOperationType.Update:
                    FilterWatermark = "Search in Updates";
                    break;

                default:
                    FilterWatermark = defaultWatermark;
                    break;
            }
        }

        private void OnSearchFilterChanged()
        {
#pragma warning disable 4014
            SearchAndRefreshAsync();
#pragma warning restore 4014
        }

        [Time]
        private async Task CountAndSearchAsync()
        {
            var selectedRepository = Navigator.SelectedRepository;

            try
            {
                using (ScopeManager<AuthenticationScope>.GetScopeManager(selectedRepository.Source.GetSafeScopeName(), () => new AuthenticationScope()))
                {
                    using (_pleaseWaitService.WaitingScope())
                    {
                        var searchSettings = SearchSettings;

                        SearchResult = await _packageQueryService.GetPackagesAsync(selectedRepository.SourceRepository, searchSettings.SearchFilter, IsPrereleaseAllowed ?? true, 25, CancellationToken.None);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to search packages");
            }
            finally
            {
                // Note: this is hack
                Navigator.SelectedRepository = selectedRepository;
            }
        }

        #endregion

        #region Commands
        public TaskCommand<IPackageDetails> PackageAction { get; private set; }

        private async Task OnPackageActionExecuteAsync(IPackageDetails package)
        {
            if (Navigator.SelectedRepository == null)
            {
                return;
            }

            if (!await OnPackageActionCanExecuteAsync(package))
            {
                return;
            }

            var operation = Navigator.SelectedRepository.OperationType;

            await _packageCommandService.ExecuteAsync(operation, package, Navigator.SelectedRepository, IsPrereleaseAllowed ?? true);

            if (_packageCommandService.IsRefreshRequired(operation))
            {
                await CountAndSearchAsync();
            }

            await RefreshCanExecuteAsync();
        }

        private async Task RefreshCanExecuteAsync()
        {
            foreach (var package in SearchResult.PackageList)
            {
                package.IsInstalled = null;

                await _packageCommandService.CanExecuteAsync(Navigator.SelectedRepository.OperationType, package);
            }
        }

        private async Task<bool> OnPackageActionCanExecuteAsync(IPackageDetails parameter)
        {
            if (Navigator.SelectedRepository == null)
            {
                return false;
            }

            return await _packageCommandService.CanExecuteAsync(Navigator.SelectedRepository.OperationType, parameter);
        }

        private bool OnPackageActionCanExecute(IPackageDetails parameter)
        {
            var canExecuteAsync = OnPackageActionCanExecuteAsync(parameter);

            canExecuteAsync.Wait(100);
            if (canExecuteAsync.IsCompleted)
            {
                return canExecuteAsync.Result;
            }

            // command must be enabled by default
            return true;
        }

        public TaskCommand CheckForUpdates { get; private set; }

        private async Task OnCheckForUpdatesExecuteAsync()
        {
            if (AvailableUpdates == null)
            {
                return;
            }

            AvailableUpdates.Clear();

            using (_pleaseWaitService.WaitingScope())
            {
                var packages = await TaskHelper.Run(() => _packagesUpdatesSearcherService.SearchForUpdates(), true);

                AvailableUpdates = new ObservableCollection<IPackageDetails>(packages);
            }

            await OnOpenUpdateWindowExecuteAsync();
        }

        public TaskCommand OpenUpdateWindow { get; private set; }

        private async Task OnOpenUpdateWindowExecuteAsync()
        {
            if (AvailableUpdates == null)
            {
                return;
            }

            await TaskHelper.Run(() => _packageBatchService.ShowPackagesBatch(AvailableUpdates, PackageOperationType.Update), true);
        }

        #endregion
    }
}
