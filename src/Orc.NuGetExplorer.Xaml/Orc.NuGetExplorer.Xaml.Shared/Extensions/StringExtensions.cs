﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System.Windows.Documents;

    public static class StringExtensions
    {
        #region Methods
        public static Inline ToInline(this string text)
        {
            return new Run(text);
        }
        #endregion
    }
}