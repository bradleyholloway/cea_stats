using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayCEASharp.Utilities;

namespace PlayCEASharp.DataModel
{
    /// <summary>
    /// Represents a match between two team.
    /// </summary>
    public class MatchResult : ConfiguredNamingObject
    {
        /// <summary>
        /// Creates a match result given the match id.
        /// </summary>
        /// <param name="matchId">The "mid" field in PlayCEA for the match.</param>
        internal MatchResult(string matchId) {
            this.MatchId = matchId;
        }

        /// <summary>
        /// Provides a deep copy constructor.
        /// </summary>
        /// <param name="match">The match to copy from.</param>
        internal MatchResult(MatchResult match)
        {
            this.MatchId = match.MatchId;
            this.HomeTeam = match.HomeTeam;
            this.AwayTeam = match.AwayTeam;
            this.Games = match.Games == null ? null : new List<Game>(match.Games);
            this.Round = match.Round;
            this.HomeGamesWon = match.HomeGamesWon;
            this.AwayGamesWon = match.AwayGamesWon;
            this.HomeGoals = match.HomeGoals;
            this.AwayGoals = match.AwayGoals;
            this.Bye = match.Bye;
            this.BracketRound = match.BracketRound;
        }

        /// <summary>
        /// Gets a string representation of the match.
        /// </summary>
        /// <returns>String representation of the match.</returns>
        public override string ToString()
        {
            return this.CustomToString();
        }

        /// <summary>
        /// The home team in the match.
        /// </summary>
        public Team HomeTeam { get; internal set; }

        /// <summary>
        /// The away team in the match. Null if is a bye.
        /// </summary>
        public Team AwayTeam { get; internal set; }

        /// <summary>
        /// The ordered list of games that are for this match.
        /// </summary>
        public List<Game> Games { get; internal set; }

        /// <summary>
        /// The match Id in PlayCEA.
        /// </summary>
        public string MatchId { get; private set; }

        /// <summary>
        /// The round that this match occurred in the bracket.
        /// </summary>
        public int Round { get; internal set; }

        /// <summary>
        /// The bracket round that this match belongs to.
        /// </summary>
        public BracketRound BracketRound { get; internal set; }

        /// <summary>
        /// The number of games the home team won.
        /// </summary>
        public int HomeGamesWon { get; internal set; }

        /// <summary>
        /// The number of games the away team won.
        /// </summary>
        public int AwayGamesWon { get; internal set; }

        /// <summary>
        /// The total number of goals the home team scored.
        /// </summary>
        public int HomeGoals { get; internal set; }

        /// <summary>
        /// The total number of goals the away team scored.
        /// </summary>
        public int AwayGoals { get; internal set; }

        /// <summary>
        /// If this week was a bye and only had 1 team.
        /// </summary>
        public bool Bye { get; internal set; }

        /// <summary>
        /// The total goal differential in favor of the home team.
        /// </summary>
        public int HomeGoalDifferential =>
            this.HomeGoals - this.AwayGoals;

        /// <summary>
        /// The total goal differential in favor of the away team.
        /// </summary>
        public int AwayGoalDifferential =>
            this.AwayGoals - this.HomeGoals;

        /// <summary>
        /// If the match has been completed or is still pending.
        /// </summary>
        public bool Completed =>
            ((this.HomeGamesWon + this.AwayGamesWon) > 0) || this.Bye;

        /// <summary>
        /// The collection of teams participating in this match.
        /// </summary>
        public List<Team> Teams { get { return this.Bye ? new List<Team>() { HomeTeam } : new List<Team>() { HomeTeam, AwayTeam }; } }
    }
}
