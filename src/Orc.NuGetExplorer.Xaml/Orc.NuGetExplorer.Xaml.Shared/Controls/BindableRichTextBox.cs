﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BindableRichTextBox.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;

    internal class BindableRichTextBox : RichTextBox
    {
        #region Properties
        public FlowDocument BindableDocument
        {
            get { return (FlowDocument) GetValue(BindableDocumentProperty); }
            set { SetValue(BindableDocumentProperty, value); }
        }

        public static readonly DependencyProperty BindableDocumentProperty =
            DependencyProperty.Register("BindableDocument", typeof(FlowDocument),
                typeof(BindableRichTextBox), new PropertyMetadata(OnDocumentChanged));
        #endregion

        #region Methods
        private static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisControl = (RichTextBox) d;

            thisControl.Document = (e.NewValue == null) ? new FlowDocument() : (FlowDocument) e.NewValue;
        }
        #endregion        
    }
}