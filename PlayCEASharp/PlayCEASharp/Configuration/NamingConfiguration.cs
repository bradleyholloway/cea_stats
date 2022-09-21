using PlayCEASharp.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayCEASharp.Utilities;

namespace PlayCEASharp.Configuration
{
    /// <summary>
    /// Configuration for modifying the nouns used for game specific terms.
    /// These only affect string representations.
    /// </summary>
    public class NamingConfiguration
    {
        /// <summary>
        /// lowercase Noun for what is a single game (part of a match).
        /// </summary>
        public string gameWord { get; set; } = "game";

        /// <summary>
        /// lowercase Nound for what is a single score in a game.
        /// </summary>
        public string scoreWord { get; set; } = "point";

        /// <summary>
        /// lowercase Noun for what is a single match between two teams.
        /// </summary>
        public string matchWord { get; set; } = "match";

        /// <summary>
        /// lowercase Noun for what is the plural of a games.
        /// </summary>
        public string gameWordPlural { get; set; }

        /// <summary>
        /// lowercase Nound for what is the plural of scores.
        /// </summary>
        public string scoreWordPlural { get; set; }

        /// <summary>
        /// lowecase Noun for what is a plural of matches.
        /// </summary>
        public string matchWordPlural { get; set; }

        /// <summary>
        /// Uppercase singular word to use for a game.
        /// </summary>
        public string GameWord { get { return gameWord.FirstCharToUpper(); } }

        /// <summary>
        /// Uppercase singular word to use for a score.
        /// </summary>
        public string ScoreWord { get { return scoreWord.FirstCharToUpper(); } }

        /// <summary>
        /// Uppercase singular word to use for a match.
        /// </summary>
        public string MatchWord { get { return matchWord.FirstCharToUpper(); } }

        /// <summary>
        /// lowercase plural for game.
        /// </summary>
        public string gameWords { get { return gameWordPlural ?? $"{gameWord}s"; } }

        /// <summary>
        /// lowercase plural for score.
        /// </summary>
        public string scoreWords { get { return scoreWordPlural ?? $"{scoreWord}s"; } }

        /// <summary>
        /// lowercase plural for match.
        /// </summary>
        public string matchWords { get { return matchWordPlural ?? $"{matchWord}s"; } }

        /// <summary>
        /// Uppercase plural for game.
        /// </summary>
        public string GameWords { get { return gameWords.FirstCharToUpper(); } }

        /// <summary>
        /// Uppercase plural for score.
        /// </summary>
        public string ScoreWords { get { return scoreWords.FirstCharToUpper(); } }

        /// <summary>
        /// Uppercase plural for match.
        /// </summary>
        public string MatchWords { get { return matchWords.FirstCharToUpper(); } }
    }
}