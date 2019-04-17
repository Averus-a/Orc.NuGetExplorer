// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectablePackageDetailsViewModel.cs" company="WildGums">
//   Copyright (c) 2008 - 2018 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;
    using Catel;
    using Catel.MVVM;

    public sealed class SelectablePackageDetailsViewModel : ViewModelBase
    {
        #region Fields
        private readonly IPackage _package;
        #endregion

        #region Constructors
        public SelectablePackageDetailsViewModel(IPackage package)
        {
            Argument.IsNotNull(() => package);

            _package = package;

            SelectPackageVersionCommand = new Command<string>(Execute);
        }
        #endregion

        #region Properties
        public Uri IconUrl => _package?.IconUrl;

        public override string Title => _package?.Title;

        public IList<string> AvailableVersions => _package?.AvailableVersions;

        public ICommand SelectPackageVersionCommand { get; }

        public bool? IsInstalled
        {
            get => _package?.IsInstalled;
            set
            {
                if (_package != null)
                {
                    _package.IsInstalled = value;
                }
            }
        }

        public string Description => _package?.Description;

        public string SelectedVersion
        {
            get => _package?.SelectedVersion;
            set
            {
                if (_package != null)
                {
                    _package.SelectedVersion = value;
                }
            }
        }

        public IPackage Package => _package;

        #endregion

        #region Methods
        private void Execute(string version)
        {
            SelectedVersion = version;
        }
        #endregion
    }
}
