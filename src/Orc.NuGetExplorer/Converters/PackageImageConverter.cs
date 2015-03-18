﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageImageConverter.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer.Converters
{
    using System;
    using Catel.IoC;
    using Catel.MVVM.Converters;

    public class PackageImageConverter : ValueConverterBase
    {
        #region Fields
        private IImageResolveService _imageResolveService;
        #endregion

        #region Properties
        private IImageResolveService ImageResolveService
        {
            get
            {
                if (_imageResolveService == null)
                {
                    var serviceLocator = this.GetServiceLocator();
                    _imageResolveService = serviceLocator.ResolveType<IImageResolveService>();
                }
                return _imageResolveService;
            }
        }
        #endregion

        #region Methods
        protected override object Convert(object value, Type targetType, object parameter)
        {
            return ImageResolveService.ResolveImageFromUri(value as Uri);
        }
        #endregion
    }
}