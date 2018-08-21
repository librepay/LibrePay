using System;
using Xunit;

namespace BitcoinPOS_App.UnitTests.TestUtility
{
    public sealed class FactOnlyInMobileAttribute : FactAttribute
    {
        public FactOnlyInMobileAttribute()
        {
            if (!IsRunningOnMono())
            {
                Skip = "Ignored on non-mobile test runner.";
            }
        }

        /// <summary>
        /// Determine if runtime is Mono.
        /// Taken from http://stackoverflow.com/questions/721161
        /// </summary>
        /// <returns>True if being executed in Mono, false otherwise.</returns>
        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }
    }
}
