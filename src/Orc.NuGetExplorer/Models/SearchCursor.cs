// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SearchCursor.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using NuGet.Protocol.Core.Types;

    public class SearchCursor
    {
        public int StartIndex { get; set; }
        public string SearchString { get; set; }
        public SearchFilter SearchFilter { get; set; }
    }
}
