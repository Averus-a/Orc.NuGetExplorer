// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RefreshToken.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using System;

    public class RefreshToken
    {
        public TimeSpan RetryAfter { get; set; }
    }
}
