using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using PlayCEASharp.Configuration;

namespace PlayCEASharp.DataModel
{
    /// <summary>
    /// Represents a collection of bracket sets comprising a "League".
    /// A league is generally identified by a unique tuple of:
    /// Game, League type (CORPORATE, COLLEGATE), YEAR, SEASON
    /// </summary>
    public class League
    {
        private BracketConfiguration configuration;

        /// <summary>
        /// The collection of bracket sets in the League.
        /// </summary>
        private List<BracketSet> bracketSets;

        /// <summary>
        /// Creates a League based on the given bracket sets.
        /// </summary>
        /// <param name="bracketSets">Collection of bracket sets for this league.</param>
        internal League(List<BracketSet> bracketSets, BracketConfiguration config)
        {
            this.configuration = config;
            if (bracketSets.Count > 0)
            {
                this.bracketSets = bracketSets;
                this.PlayerDiscordLookup = new Dictionary<string, Team>();
                foreach (Team team in this.Bracket.Teams)
                {
                    foreach (Player player in team.Players)
                    {
                        this.PlayerDiscordLookup[player.DiscordId] =  team;
                    }
                }
                this.NextMatchLookup = new Dictionary<Team, MatchResult>();

                foreach (MatchResult result in this.Bracket.Rounds.Last().SelectMany(r => r.Matches))
                {
                    this.NextMatchLookup[result.HomeTeam] = result;
                    if (result.AwayTeam != null)
                    {
                        this.NextMatchLookup[result.AwayTeam] = result;
                    }
                }
            }
        }

        /// <summary>
        /// Provides a precomputed dictionary from discriminated discord name to which team they are on.
        /// </summary>
        public Dictionary<string, Team> PlayerDiscordLookup { get; }

        /// <summary>
        /// Provides a precomputed dictionary from each team to their next match.
        /// </summary>
        public Dictionary<Team, MatchResult> NextMatchLookup { get; }

        /// <summary>
        /// Gets the most recent bracket set for the league.
        /// </summary>
        public BracketSet Bracket { get { return bracketSets.Last(); } }

        /// <summary>
        /// Gets the collection of all bracket sets for the league.
        /// </summary>
        public List<BracketSet> Brackets => this.bracketSets;

        /// <summary>
        /// Lookup the stage for a given round.
        /// </summary>
        /// <param name="roundName">The round to look for.</param>
        /// <returns>The stage name for the given round if present.</returns>
        public string StageLookup(string roundName)
        {
            return this.configuration.StageLookup(roundName);
        }

    }
}
