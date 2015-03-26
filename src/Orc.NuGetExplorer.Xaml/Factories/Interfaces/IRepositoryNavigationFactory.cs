// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRepositoryNavigationFactory.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    internal interface IRepositoryNavigationFactory
    {
        #region Methods
        RepositoryNavigator CreateRepoNavigator();
        #endregion
    }
}