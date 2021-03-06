﻿namespace Orc.NuGetExplorer.ViewModels
{
    using System.ComponentModel;
    using System.Threading.Tasks;
    using Catel;
    using Catel.Fody;
    using Catel.Logging;
    using Catel.MVVM;
    using NuGet.Versioning;
    using Orc.NuGetExplorer.Enums;
    using Orc.NuGetExplorer.Models;

    internal class PageItemViewModel : ViewModelBase
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly IRepositoryContextService _repositoryService;

        public PageItemViewModel(NuGetPackage package, IRepositoryContextService repositoryService)
        {
            Argument.IsNotNull(() => package);
            Argument.IsNotNull(() => repositoryService);

            _repositoryService = repositoryService;

            Package = package;
        }

        [Model(SupportIEditableObject = false)]
        [Expose("Title")]
        [Expose("Description")]
        [Expose("Summary")]
        [Expose("DownloadCount")]
        [Expose("Authors")]
        [Expose("IconUrl")]
        [Expose("Identity")]
        [Expose("IsChecked")]
        public NuGetPackage Package { get; set; }

        [ViewModelToModel]
        public PackageStatus Status { get; set; }

        public bool IsDownloadCountShowed { get; private set; }

        public bool CanBeAddedInBatchOperation { get; set; }

        public bool IsSecondaryVersionShowed { get; private set; } = false;

        public NuGetVersion FirstVersion { get; set; }

        public NuGetVersion SecondaryVersion { get; set; }

        protected override Task InitializeAsync()
        {
            var packageOrigin = Package.FromPage;

            IsDownloadCountShowed = packageOrigin == MetadataOrigin.Browse;
            CanBeAddedInBatchOperation = packageOrigin == MetadataOrigin.Updates;

            FirstVersion = Package.Identity.Version;

            switch (packageOrigin)
            {
                case MetadataOrigin.Browse:
                    IsSecondaryVersionShowed = true;
                    SecondaryVersion = Package.InstalledVersion;
                    break;

                case MetadataOrigin.Updates:
                    IsSecondaryVersionShowed = true;
                    FirstVersion = Package.InstalledVersion;
                    SecondaryVersion = Package.LastVersion;
                    break;
            }

            return base.InitializeAsync();
        }

        protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, nameof(Package.InstalledVersion)) && Package.FromPage == MetadataOrigin.Browse)
            {
                SecondaryVersion = Package.InstalledVersion;
            }

            base.OnModelPropertyChanged(sender, e);
        }
    }
}
