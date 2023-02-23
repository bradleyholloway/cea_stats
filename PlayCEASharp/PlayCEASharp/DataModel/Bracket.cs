using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEASharp.DataModel
{
    /// <summary>
    /// A bracket in PlayCEA. This contains a List of BracketRounds, as well as a List of Teams.
    /// </summary>
    public class Bracket
    {
        /// <summary>
        /// Creates a new Bracket object with the given bracketId.
        /// This does not populate any data automatically.
        /// </summary>
        /// <param name="bracketId">The id for this bracket.</param>
        internal Bracket(string bracketId)
        {
            this.BracketId = bracketId;
        }

        /// <summary>
        /// The BracketId in PlayCEA
        /// </summary>
        public string BracketId { get; }

        /// <summary>
        /// Ordered list of the rounds in the bracket.
        /// </summary>
        public List<BracketRound> Rounds { get; } = new List<BracketRound>();

        /// <summary>
        /// Collection of the teams who are registered in this bracket.
        /// </summary>
        public List<Team> Teams { get; } = new List<Team>();

        /// <summary>
        /// The name of the bracket in PlayCEA.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// The game id of the game for this bracket.
        /// </summary>
        public string Game { get; internal set; }

        /// <summary>
        /// When meta is present, these are the teams in the top group.
        /// </summary>
        public List<Team> TopMetaTeams { get; internal set; }

        /// <summary>
        /// When meta is present these are the teams in the middle group.
        /// </summary>
        public List<Team> MidMetaTeams { get; internal set; }

        /// <summary>
        /// When meta is present these are the teams in the bottom group.
        /// </summary>
        public List<Team> BotMetaTeams { get; internal set; }

        /// <summary>
        /// Backlink to the tournament linked to this bracket.
        /// </summary>
        public Tournament Tournament { get; internal set; }
    }
}
