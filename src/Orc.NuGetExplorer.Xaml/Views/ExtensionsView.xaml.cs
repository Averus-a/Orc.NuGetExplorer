﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtensionsView.xaml.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer.Views
{
    using System.Windows;
    using Catel.MVVM.Views;

    /// <summary>
    /// Interaction logic for ExtensionsView.xaml.
    /// </summary>
    internal partial class ExtensionsView
    {
        #region Constructors
        static ExtensionsView()
        {
            typeof (ExtensionsView).AutoDetectViewPropertiesToSubscribe();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionsView"/> class.
        /// </summary>
        public ExtensionsView()
        {
            InitializeComponent();
        }
        #endregion

        #region Properties
        [ViewToViewModel(MappingType = ViewToViewModelMappingType.ViewToViewModel)]
        public IRepository SelectedRepository
        {
            get { return (IRepository)GetValue(SelectedRepositoryProperty); }
            set { SetValue(SelectedRepositoryProperty, value); }
        }

        public static readonly DependencyProperty SelectedRepositoryProperty = DependencyProperty.Register("SelectedRepository", typeof(IRepository),
            typeof(ExtensionsView), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        #endregion
    }
}