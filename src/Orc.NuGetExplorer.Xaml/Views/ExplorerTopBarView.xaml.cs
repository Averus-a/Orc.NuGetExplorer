﻿namespace Orc.NuGetExplorer.Views
{
    using System.Windows;
    using Catel.Windows.Controls;

    /// <summary>
    /// Interaction logic for ExplorerTopBarView.xaml
    /// </summary>
    internal partial class ExplorerTopBarView : UserControl
    {
        public ExplorerTopBarView()
        {
            InitializeComponent();
        }

        public DependencyObject UsedOn
        {
            get { return (DependencyObject)GetValue(UsedOnProperty); }
            set { SetValue(UsedOnProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TabSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UsedOnProperty =
            DependencyProperty.Register("UsedOn", typeof(DependencyObject), typeof(ExplorerTopBarView), new PropertyMetadata(null));
    }
}
