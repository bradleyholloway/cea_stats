using PlayCEASharp.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEASharp.RequestManagement
{
    /// <summary>
    /// Caches objects that are uniquely identified by id to provide easy and consistant views of the objects and references across callers.
    /// </summary>
    internal static class ResourceCache
    {
        /// <summary>
        /// Backing cache for teams.
        /// </summary>
        private static readonly GenericCache<Team> teamCache = new GenericCache<Team>((id) => new Team(id));

        /// <summary>
        /// Backing cache for bracket rounds.
        /// </summary>
        private static readonly GenericCache<BracketRound> bracketRoundCache = new GenericCache<BracketRound>((id) => new BracketRound(id));

        /// <summary>
        /// Backing cache for tournaments.
        /// </summary>
        private static readonly GenericCache<Tournament> tournamentCache = new GenericCache<Tournament>((id) => new Tournament(id));

        /// <summary>
        /// Backing cache for brackets.
        /// </summary>
        private static readonly GenericCache<Bracket> bracketCache = new GenericCache<Bracket>((id) => new Bracket(id));

        /// <summary>
        /// Gets the team with the given id.
        /// </summary>
        /// <param name="id">The team id.</param>
        /// <returns>The team with the given id.</returns>
        internal static Team GetTeam(string id)
        {
            return teamCache.Get(id);
        }

        /// <summary>
        /// Gets the bracket with the given id.
        /// </summary>
        /// <param name="id">The bracket id.</param>
        /// <returns>The bracket with the given id.</returns>
        internal static BracketRound GetBracketRound(string id) {
            return bracketRoundCache.Get(id);
        }

        /// <summary>
        /// Gets the tournament with the given id.
        /// </summary>
        /// <param name="id">The tournament id.</param>
        /// <returns>The tournament with the given id.</returns>
        internal static Tournament GetTournament(string id) {
            return tournamentCache.Get(id);
        }

        /// <summary>
        /// Gets the bracket with the given id.
        /// </summary>
        /// <param name="id">The bracket id.</param>
        /// <returns>The bracket with the given id.</returns>
        internal static Bracket GetBracket(string id) {
            return bracketCache.Get(id);
        }
    }
}
