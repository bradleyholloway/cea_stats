using PlayCEASharp.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEASharp.Configuration
{
    /// <summary>
    /// The configuration to scope to which tournaments to analyze.
    /// Additionally contains information for how to order the brackets and stages.
    /// </summary>
    public class MatchingConfiguration
    {
        /// <summary>
        /// The gameId to filter to.
        /// </summary>
        public string gameId { get; set; }

        /// <summary>
        /// The league to filter to.
        /// </summary>
        public string league { get; set; } = "CORPORATE";

        /// <summary>
        /// The year to filter to.
        /// </summary>
        public string year { get; set; }

        /// <summary>
        /// The season to filter to.
        /// </summary>
        public string season { get; set; }

        /// <summary>
        /// The mapping from stage index to a name for the stage.
        /// </summary>
        public Dictionary<int, string> stageNames { get; set; }

        /// <summary>
        /// Mapping from keywords of bracket/tournament names to which stage they belong to.
        /// </summary>
        public Dictionary<string, int> stageKeywords { get; set; }

        /// <summary>
        /// Mapping from bracket/tournament names to how they should be ordered to others in the same stage.
        /// </summary>
        public Dictionary<string, int> orderKeywords { get; set; }
    }
}
