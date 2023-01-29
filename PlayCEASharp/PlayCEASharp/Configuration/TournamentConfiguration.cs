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
    /// </summary>
    public class TournamentConfiguration
    {
        /// <summary>
        /// The string key for this tournament, needs to be unique.
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// The Matching configuration for this tournament.
        /// </summary>
        public MatchingConfiguration matchingConfig { get; set; }

        /// <summary>
        /// The naming configuration for this tournament.
        /// </summary>
        public NamingConfiguration namingConfig { get; set; }
    }
}
