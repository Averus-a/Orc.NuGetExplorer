﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageOperationType.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    public enum PackageOperationType
    {
        None = 0, // default value
        Install,
        Uninstall,
        Update
    }
}
