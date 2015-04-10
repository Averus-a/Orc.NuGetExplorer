﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAuthenticationProvider.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System;

    internal interface IAuthenticationProvider
    {
        #region Methods
        AuthenticationCredentials GetCredentials(Uri uri, bool previousCredentialsFailed);
        #endregion
    }
}