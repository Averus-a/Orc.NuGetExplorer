// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageSourceSettingViewModel.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer.ViewModels
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Catel;
    using Catel.Collections;
    using Catel.Data;
    using Catel.Fody;
    using Catel.MVVM;
    using Catel.Scoping;
    using Catel.Threading;
    using NuGet.Configuration;
    using Scopes;

    internal class PackageSourceSettingViewModel : ViewModelBase
    {
        #region Fields
        private readonly INuGetFeedVerificationService _nuGetFeedVerificationService;

        private bool _ignoreNextPackageUpdate;
        #endregion

        #region Constructors
        public PackageSourceSettingViewModel(INuGetFeedVerificationService nuGetFeedVerificationService)
        {
            Argument.IsNotNull(() => nuGetFeedVerificationService);

            _nuGetFeedVerificationService = nuGetFeedVerificationService;

            Add = new Command(OnAddExecute);
            Remove = new Command(OnRemoveExecute, OnRemoveCanExecute);
            MoveUp = new Command(OnMoveUpExecute, OnMoveUpCanExecute);
            MoveDown = new Command(OnMoveDownExecute, OnMoveDownCanExecute);
        }
        #endregion

        #region Properties
        public IList<EditablePackageSource> EditablePackageSources { get; private set; }
        public IEnumerable<PackageSource> PackageSources { get; set; }

        [Model(SupportIEditableObject = false)]
        [Expose(nameof(EditablePackageSource.Name))]
        [Expose(nameof(EditablePackageSource.Source))]
        public EditablePackageSource SelectedPackageSource { get; set; }

        public string DefaultFeed { get; set; }
        public string DefaultSourceName { get; set; }
        #endregion

        #region Methods
        protected override async Task InitializeAsync()
        {
            if (PackageSources != null)
            {
                OnPackageSourcesChanged();
            }

            await base.InitializeAsync();
        }

        private void OnPackageSourcesChanged()
        {
            EditablePackageSources = new FastObservableCollection<EditablePackageSource>(PackageSources.Select(x =>
                new EditablePackageSource
                {
                    IsEnabled = x.IsEnabled,
                    Name = x.Name,
                    Source = x.Source
                }));

            if (_ignoreNextPackageUpdate)
            {
                return;
            }

            VerifyAll();
        }

        protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnModelPropertyChanged(sender, e);
            if (string.Equals(e.PropertyName, nameof(EditablePackageSource.Name)))
            {
                VerifyAll();
            }

            if (!string.Equals(e.PropertyName, nameof(EditablePackageSource.Source)))
            {
                return;
            }

            var selectedPackageSource = SelectedPackageSource;

            if (selectedPackageSource?.IsValid == null)
            {
                return;
            }

            selectedPackageSource.IsValid = false;

#pragma warning disable 4014
            VerifyPackageSourceAsync(selectedPackageSource);
#pragma warning restore 4014
        }

        protected override async Task<bool> SaveAsync()
        {
            var editablePackageSource = EditablePackageSources;
            if (editablePackageSource == null)
            {
                return false;
            }

            if (editablePackageSource.Any(x => x.IsValid == null))
            {
                return false;
            }

            _ignoreNextPackageUpdate = true;

            PackageSources = editablePackageSource.Select(x =>
            {
                using (ScopeManager<AuthenticationScope>.GetScopeManager(x.Source.GetSafeScopeName(), () => new AuthenticationScope(false)))
                {
                    var packageSource = new PackageSource(x.Source, x.Name, x.IsEnabled, false);
                    return packageSource;
                }
            }).ToArray();

            return await base.SaveAsync();
        }

        protected override void ValidateFields(List<IFieldValidationResult> validationResults)
        {
            base.ValidateFields(validationResults);

            if (SelectedPackageSource == null || (SelectedPackageSource.IsValid ?? true))
            {
                return;
            }

            if (!(SelectedPackageSource.IsValidName ?? false))
            {
                validationResults.Add(FieldValidationResult.CreateError(nameof(EditablePackageSource.Name),
                    $"Package source name '{SelectedPackageSource.Name}' is empty or not unique."));
            }

            if (!(SelectedPackageSource.IsValidSource ?? false))
            {
                validationResults.Add(FieldValidationResult.CreateError(nameof(EditablePackageSource.Source), 
                    $"Package source '{SelectedPackageSource.Source}' is invalid."));
            }
        }

        protected override void ValidateBusinessRules(List<IBusinessRuleValidationResult> validationResults)
        {
            base.ValidateBusinessRules(validationResults);

            if (EditablePackageSources != null && EditablePackageSources.Any(x => x.IsValid.HasValue && !x.IsValid.Value))
            {
                validationResults.Add(BusinessRuleValidationResult.CreateError("Some package sources are invalid."));
            }
        }

        private async Task VerifyPackageSourceAsync(EditablePackageSource packageSource, bool force = false)
        {
            if (packageSource == null || packageSource.IsValid == null)
            {
                return;
            }

            if (!force && string.Equals(packageSource.Source, packageSource.PreviousSourceValue))
            {
                return;
            }

            packageSource.IsValid = null;

            string feedToValidate;
            string nameToValidate;
            bool isValidUrl;
            bool isValidName;

            do
            {
                feedToValidate = packageSource.Source;
                nameToValidate = packageSource.Name;

                var namesCount = EditablePackageSources.Count(x => string.Equals(nameToValidate, x.Name));

                isValidName = !string.IsNullOrWhiteSpace(nameToValidate) && namesCount == 1;

                var validate = feedToValidate;
                var feedVerificationResult = await _nuGetFeedVerificationService.VerifyFeedAsync(validate, true, CancellationToken.None);

                packageSource.FeedVerificationResult = feedVerificationResult;
                isValidUrl = feedVerificationResult != FeedVerificationResult.Invalid && feedVerificationResult != FeedVerificationResult.Unknown;

            } while (!string.Equals(feedToValidate, packageSource.Source) && !string.Equals(nameToValidate, packageSource.Name));

            packageSource.PreviousSourceValue = packageSource.Source;
            packageSource.IsValidSource = isValidUrl;
            packageSource.IsValidName = isValidName;

            Validate(true);
        }

        private bool CanMoveToStep(int step)
        {
            var selectedPackageSource = SelectedPackageSource;
            if (selectedPackageSource == null)
            {
                return false;
            }

            var editablePackageSources = EditablePackageSources;

            var index = editablePackageSources.IndexOf(selectedPackageSource);

            var newIndex = index + step;

            return newIndex >= 0 && newIndex < editablePackageSources.Count;
        }

        private void MoveToStep(int step)
        {
            var selectedPackageSource = SelectedPackageSource;
            if (selectedPackageSource == null)
            {
                return;
            }

            var editablePackageSources = EditablePackageSources;

            var index = editablePackageSources.IndexOf(selectedPackageSource);

            EditablePackageSources.RemoveAt(index);
            EditablePackageSources.Insert(index + step, selectedPackageSource);

            SelectedPackageSource = selectedPackageSource;
        }

        private void VerifyAll()
        {
            foreach (var packageSource in EditablePackageSources)
            {
#pragma warning disable 4014
                VerifyPackageSourceAsync(packageSource, true);
#pragma warning restore 4014
            }
        }
        #endregion

        #region Commands
        public Command MoveUp { get; private set; }

        private void OnMoveUpExecute()
        {
            MoveToStep(-1);
        }

        private bool OnMoveUpCanExecute()
        {
            return CanMoveToStep(-1);
        }

        public Command MoveDown { get; private set; }

        private void OnMoveDownExecute()
        {
            MoveToStep(1);
        }

        private bool OnMoveDownCanExecute()
        {
            return CanMoveToStep(1);
        }

        public Command Add { get; private set; }

        private void OnAddExecute()
        {
            var packageSource = new EditablePackageSource
            {
                IsEnabled = true,
                Name = DefaultSourceName,
                Source = DefaultFeed,
                IsValid = true
            };

            EditablePackageSources.Add(packageSource);
            SelectedPackageSource = packageSource;

            VerifyAll();
        }

        public Command Remove { get; private set; }

        private void OnRemoveExecute()
        {
            if (SelectedPackageSource == null)
            {
                return;
            }

            var index = EditablePackageSources.IndexOf(SelectedPackageSource);
            if (index < 0)
            {
                return;
            }

            EditablePackageSources.RemoveAt(index);

            SelectedPackageSource = index < EditablePackageSources.Count 
                ? EditablePackageSources[index] 
                : EditablePackageSources.LastOrDefault();

            VerifyAll();
        }

        private bool OnRemoveCanExecute()
        {
            return SelectedPackageSource != null;
        }
        #endregion
    }
}
