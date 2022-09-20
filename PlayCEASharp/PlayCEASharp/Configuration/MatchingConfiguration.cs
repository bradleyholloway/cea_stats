using PlayCEASharp.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEASharp.Configuration
{
    public class MatchingConfiguration
    {
        public string gameId { get; set; }
        public string league { get; set; } = "CORPORATE";
        public string year { get; set; }
        public string season { get; set; }

        public Dictionary<int, string> stageNames { get; set; }

        public Dictionary<string, int> stageKeywords { get; set; }

        public Dictionary<string, int> orderKeywords { get; set; }
    }
}
