using PlayCEASharp.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEASharp.Analysis
{
    /// <summary>
    /// Helper class to match round names to the stage they belong to.
    /// </summary>
    public static class StageMatcher
    {
        /// <summary>
        /// Lookup the stage for a given round.
        /// </summary>
        /// <param name="roundName">The round to look for.</param>
        /// <returns></returns>
        public static string Lookup(string roundName)
        {
            string str;
            return (ConfigurationManager.Configuration.stageConfiguration.TryGetValue(roundName, out str) ? str : "default_stage");
        }
    }
}
