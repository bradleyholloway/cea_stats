using PlayCEASharp.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayCEASharp.Utilities;

namespace PlayCEASharp.Configuration
{
    public class NamingConfiguration
    {

        public string gameWord { get; set; } = "game";
        public string scoreWord { get; set; } = "point";
        public string matchWord { get; set; } = "match";

        public string gameWordPlural { get; set; }
        public string scoreWordPlural { get; set; }
        public string matchWordPlural { get; set; }
        
        public string GameWord { get { return gameWord.FirstCharToUpper(); } }
        public string ScoreWord { get { return scoreWord.FirstCharToUpper(); } }
        public string MatchWord { get { return matchWord.FirstCharToUpper(); } }

        public string gameWords { get { return gameWordPlural ?? $"{gameWord}s"; } }
        public string scoreWords { get { return scoreWordPlural ?? $"{scoreWord}s"; } }
        public string matchWords { get { return matchWordPlural ?? $"{matchWord}s"; } }

        public string GameWords { get { return gameWords.FirstCharToUpper(); } }
        public string ScoreWords { get { return scoreWords.FirstCharToUpper(); } }
        public string MatchWords { get { return matchWords.FirstCharToUpper(); } }
    }
}