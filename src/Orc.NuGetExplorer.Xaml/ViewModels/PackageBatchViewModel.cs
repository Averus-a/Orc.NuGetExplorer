// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageBatchViewModel.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Catel;
    using Catel.Fody;
    using Catel.MVVM;
    using Catel.Threading;
    using NuGet.Protocol.Core.Types;

    internal class PackageBatchViewModel : ViewModelBase
    {
        #region Fields
        private readonly IPackageCommandService _packageCommandService;
        private readonly IPackageOperationContextService _packageOperationContextService;
        #endregion

        #region Constructors
        public PackageBatchViewModel(PackagesBatch packagesBatch, IPackageCommandService packageCommandService, IPackageOperationContextService packageOperationContextService)
        {
            Argument.IsNotNull(() => packagesBatch);
            Argument.IsNotNull(() => packageCommandService);
            Argument.IsNotNull(() => packageOperationContextService);

            _packageCommandService = packageCommandService;
            _packageOperationContextService = packageOperationContextService;

            PackagesBatch = packagesBatch;
            AccentColorHelper.CreateAccentColorResourceDictionary();

            ActionName = _packageCommandService.GetActionName(packagesBatch.OperationType);
            PluralActionName = _packageCommandService.GetPluralActionName(packagesBatch.OperationType);

            PackageAction = new TaskCommand(OnPackageActionExecuteAsync, OnPackageActionCanExecute);
            ApplyAll = new TaskCommand(OnApplyAllExecuteAsync, OnApplyAllCanExecute);
        }
        #endregion

        #region Properties
        [Model]
        [Expose(nameof(NuGetExplorer.PackagesBatch.PackageList))]
        public PackagesBatch PackagesBatch { get; set; }

        public string ActionName { get; private set; }
        public string PluralActionName { get; private set; }
        public IPackageSearchMetadata SelectedPackage { get; set; }
        #endregion

        #region Methods
        protected override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            await RefreshCanExecuteAsync();

            SetTitle();
        }

        private void SetTitle()
        {
            switch (PackagesBatch.OperationType)
            {
                case PackageOperationType.Install:
                    Title = "Installing packages";
                    break;

                case PackageOperationType.Uninstall:
                    Title = "Uninstalling packages";
                    break;

                case PackageOperationType.Update:
                    Title = "Package updates";
                    break;
            }
        }
        #endregion

        #region Commands
        public TaskCommand ApplyAll { get; private set; }

        private async Task OnApplyAllExecuteAsync()
        {
            if (!await OnApplyAllCanExecuteAsync())
            {
                return;
            }

            var packages = await GetPackagesForOperationAsync(PackagesBatch.PackageList);
            using (_packageOperationContextService.UseOperationContext(PackagesBatch.OperationType, packages))
            {
                foreach (var package in packages)
                {
                    await _packageCommandService.ExecuteAsync(PackagesBatch.OperationType, package);

                    await RefreshCanExecuteAsync();
                }
            }
        }

        private async Task<IPackageSearchMetadata[]> GetPackagesForOperationAsync(IReadOnlyList<IPackageSearchMetadata> packageDetailsList)
        {
            var result = new List<IPackageSearchMetadata>(packageDetailsList.Count);

            foreach (var packageDetails in packageDetailsList)
            {
                if (!await _packageCommandService.CanExecuteAsync(PackagesBatch.OperationType, packageDetails))
                {
                    continue;
                }

                result.Add(packageDetails);
            }

            return result.ToArray();
        }

        private async Task<bool> OnApplyAllCanExecuteAsync()
        {
            foreach (var packageDetails in PackagesBatch.PackageList)
            {
                if (!await _packageCommandService.CanExecuteAsync(PackagesBatch.OperationType, packageDetails))
                {
                    return false;
                }
            }

            return true;
        }

        private bool OnApplyAllCanExecute()
        {
            var canExecuteAsync = (Func<Task<bool>>)OnApplyAllCanExecuteAsync;

            return canExecuteAsync.ExtractBooleanResult(true, false, 100);
        }

        public TaskCommand PackageAction { get; set; }

        private async Task OnPackageActionExecuteAsync()
        {
            if (!await OnPackageActionCanExecuteAsync())
            {
                return;
            }

            await _packageCommandService.ExecuteAsync(PackagesBatch.OperationType, SelectedPackage);

            await RefreshCanExecuteAsync();
        }

        private Task<bool> OnPackageActionCanExecuteAsync()
        {
            return _packageCommandService.CanExecuteAsync(PackagesBatch.OperationType, SelectedPackage);
        }

        private bool OnPackageActionCanExecute()
        {
            var canExecuteAsync = (Func<Task<bool>>)OnPackageActionCanExecuteAsync;

            return canExecuteAsync.ExtractBooleanResult(true, false, 100);
        }

        private async Task RefreshCanExecuteAsync()
        {
            foreach (var package in PackagesBatch.PackageList)
            {
               // package.IsInstalled = null;
                await _packageCommandService.CanExecuteAsync(PackagesBatch.OperationType, package);
            }
        }
        #endregion
    }
}
