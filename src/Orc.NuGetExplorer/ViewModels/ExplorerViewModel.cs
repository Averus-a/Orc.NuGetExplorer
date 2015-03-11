﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExplorerViewModel.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer.ViewModels
{
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media;
    using Catel;
    using Catel.Fody;
    using Catel.IoC;
    using Catel.Logging;
    using Catel.MVVM;
    using Extensions;
    using NuGet;

    internal class ExplorerViewModel : ViewModelBase
    {
        #region Fields
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private readonly IRepositoryNavigationService _repositoryNavigationService;
        #endregion

        #region Constructors
        public ExplorerViewModel(IRepositoryNavigationService repositoryNavigationService)
        {
            Argument.IsNotNull(() => repositoryNavigationService);

            _repositoryNavigationService = repositoryNavigationService;

            Navigator = _repositoryNavigationService.GetNavigator();

            var accentColor = GetAccentColorBrush().Color;
            accentColor.CreateAccentColorResourceDictionary();
        }
        #endregion

        #region Properties
        [Model]
        [Expose("RepoCategories")]
        public RepositoryNavigator Navigator { get; private set; }

        [ViewModelToModel("Navigator")]
        public NamedRepository SelectedNamedRepository { get; set; }
        #endregion

        #region Methods
        protected override async Task Initialize()
        {
            await base.Initialize();

            Navigator.SelectedRepositoryCategory = Navigator.RepoCategories.FirstOrDefault();
            var selectedRepositoryCategory = Navigator.SelectedRepositoryCategory;
            if (selectedRepositoryCategory != null)
            {
                selectedRepositoryCategory.IsSelected = true;
                SelectedNamedRepository = selectedRepositoryCategory.Repos.FirstOrDefault();
            }
        }

        private SolidColorBrush GetAccentColorBrush()
        {
            var accentColorBrush = Application.Current.TryFindResource("AccentColorBrush") as SolidColorBrush;
            if (accentColorBrush != null)
            {
                return accentColorBrush;
            }
            return new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
        }
        #endregion
    }
}