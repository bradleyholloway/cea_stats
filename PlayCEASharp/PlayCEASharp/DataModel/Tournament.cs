using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEASharp.DataModel
{
    /// <summary>
    /// Represents a tournament object in PlayCEA.
    /// </summary>
    public class Tournament
    {
        /// <summary>
        /// Creates a tournament with the given id.
        /// </summary>
        /// <param name="tournamentId">the "tmid" field in playCEA.</param>
        public Tournament(string tournamentId)
        {
            this.TournamentId = tournamentId;
            this.Brackets = new List<Bracket>();
            this.Teams = new List<Team>();
        }

        /// <summary>
        /// The unique id for this tournament.
        /// </summary>
        public string TournamentId { get; }

        /// <summary>
        /// Message associated with the tournament.
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// Any metadata associated with the tournament. Unstructured.
        /// </summary>
        public object Metadata { get; internal set; }

        /// <summary>
        /// The unique identifier for the game being played for this tournament.
        /// </summary>
        public string GameId { get; internal set; }

        /// <summary>
        /// The full name of the game being played.
        /// </summary>
        public string GameName { get; internal set; }

        /// <summary>
        /// The name of the tournament.
        /// </summary>
        public string TournamentName { get; internal set; }

        /// <summary>
        /// If this tournament is currently live.
        /// </summary>
        public bool Live { get; internal set; }

        /// <summary>
        /// If this tournament is current.
        /// </summary>
        public bool Current { get; internal set; }

        /// <summary>
        /// The collection of brackets this tournament references.
        /// </summary>
        public List<Bracket> Brackets { get; internal set; }

        /// <summary>
        /// The collection of teams in the tournament.
        /// </summary>
        public List<Team> Teams { get; internal set; }

        /// <summary>
        /// The league that this tournament is for.
        /// </summary>
        public string SeasonLeague { get; internal set; }

        /// <summary>
        /// The year this tournament is in.
        /// </summary>
        public string SeasonYear { get; internal set; }

        /// <summary>
        /// The season (SPRING/FALL) that this tournament is for.
        /// </summary>
        public string SeasonSeason { get; internal set; }
    }
}
