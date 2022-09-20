using PlayCEASharp.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEASharp.Configuration
{
    public class BracketConfiguration
    {
        public string[][] bracketSets { get; set; }

        public Dictionary<string, string> stageConfiguration { get; set; }

        public StageGroup[] stageGroups { get; set; }
    }
}
