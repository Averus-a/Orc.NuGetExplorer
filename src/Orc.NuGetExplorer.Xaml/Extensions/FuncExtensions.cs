// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FuncExtensions.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using System;
    using System.Threading.Tasks;

    public static class FuncExtensions
    {
        public static bool ExtractBooleanResult(this Func<Task<bool>> func, bool defaultSuccessValue, bool defaultFaultedValue, int millisecondTimeOut)
        {
            var task = func();

            task.Wait(millisecondTimeOut);
            if (task.IsFaulted || task.IsCanceled)
            {
                return defaultFaultedValue;
            }

            if (task.IsCompleted)
            {
                return task.Result;
            }

            return defaultSuccessValue;
        }
    }
}
