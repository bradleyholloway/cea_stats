using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayCEASharp.Utilities;

namespace PlayCEASharp.DataModel
{
    /// <summary>
    /// Represents a single game in PlayCEA.
    /// </summary>
    public class Game
    {
        /// <summary>
        /// Creates a game with the given game id in PlayCEA.
        /// </summary>
        /// <param name="id">The gid field in PlayCEA.</param>
        internal Game(string id)
        {
            this.GameId = id;
        }

        /// <summary>
        /// Gets a string representation of the game result.
        /// </summary>
        /// <returns>A string representing the game.</returns>
        public override string ToString()
        {
            return this.CustomToString();
        }

        /// <summary>
        /// The gid in PlayCEA.
        /// </summary>
        public string GameId { get; }

        /// <summary>
        /// The home team of the game.
        /// </summary>
        public Team HomeTeam { get; internal set; }

        /// <summary>
        /// The away team of the game. Null if in a Bye.
        /// </summary>
        public Team AwayTeam { get; internal set; }

        /// <summary>
        /// The score that the home team scored.
        /// </summary>
        public int HomeScore { get; internal set; }

        /// <summary>
        /// The score that the away team scored.
        /// </summary>
        public int AwayScore { get; internal set; }

        /// <summary>
        /// Score differential in favor of the home team.
        /// </summary>
        public int HomeScoreDifferential =>
            this.HomeScore - this.AwayScore;

        /// <summary>
        /// Score differential in favor of the away team.
        /// </summary>
        public int AwayScoreDifferential =>
            this.AwayScore - this.HomeScore;
    }
}
