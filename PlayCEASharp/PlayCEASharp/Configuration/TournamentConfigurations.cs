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
    public class TournamentConfigurations
    {
        /// <summary>
        /// List of MatchingConfigurations to listen to.
        /// </summary>
        public List<TournamentConfiguration> configurations { get; set; }

        /// <summary>
        /// Optional override to the default production cea endpoint.
        /// </summary>
        public string endpoint { get; set; }
    }
}
