﻿using System;
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
        /// The collection of teams in the League.
        /// </summary>
        private List<Team> teams;

        /// <summary>
        /// The Game Id.
        /// </summary>
        private string gameId;

        /// <summary>
        /// Creates a League based on the given bracket sets.
        /// </summary>
        /// <param name="bracketSets">Collection of bracket sets for this league.</param>
        /// <param name="config">The BracketConfiguration to use for the League.</param>
        /// <param name="tournaments">List of tournaments used in this League</param>
        internal League(List<BracketSet> bracketSets, BracketConfiguration config, List<Tournament> tournaments)
        {
            this.configuration = config;
            this.bracketSets = bracketSets;
            this.PlayerDiscordLookup = new Dictionary<string, List<Team>>();
            this.PlayerDiscordIdLookup = new Dictionary<ulong, List<Team>>();
            this.NextMatchLookup = new Dictionary<Team, MatchResult>();
            this.teams = new List<Team>();

            foreach (Tournament t in tournaments)
            {
                this.gameId = t.GameId;
                foreach (Team team in t.Teams)
                {
                    if (!this.teams.Contains(team))
                    {
                        this.teams.Add(team);
                        foreach (Player player in team.Players)
                        {
                            this.PlayerDiscordLookup[player.DiscordId] = this.PlayerDiscordLookup.GetValueOrDefault(player.DiscordId, new List<Team>());
                            this.PlayerDiscordLookup[player.DiscordId].Add(team);

                            this.PlayerDiscordIdLookup[player.DiscordUID] = this.PlayerDiscordIdLookup.GetValueOrDefault(player.DiscordUID, new List<Team>());
                            this.PlayerDiscordIdLookup[player.DiscordUID].Add(team);
                        }
                    }
                }
            }

            if (bracketSets.Count > 0)
            {
                foreach (MatchResult result in this.Bracket.Rounds.Last().SelectMany(r => r.Matches))
                {
                    this.NextMatchLookup[result.HomeTeam] = result;
                    if (result.AwayTeam != null)
                    {
                        this.NextMatchLookup[result.AwayTeam] = result;
                    }
                }
            }

            this.MatchLookup = this.Brackets.SelectMany(b => b.Rounds).SelectMany(r => r).SelectMany(r => r.Matches).ToDictionary(m => m.MatchId, m => m);
        }

        /// <summary>
        /// Provides a precomputed dictionary from discriminated discord name to which team they are on.
        /// </summary>
        public Dictionary<string, List<Team>> PlayerDiscordLookup { get; }

        /// <summary>
        /// Provides a precomputed dictionary from discriminated discord name to which team they are on.
        /// </summary>
        public Dictionary<ulong, List<Team>> PlayerDiscordIdLookup { get; }

        /// <summary>
        /// Provides a precomputed dictionary from each team to their next match.
        /// </summary>
        public Dictionary<Team, MatchResult> NextMatchLookup { get; }

        /// <summary>
        /// Looks up a match given a matchid.
        /// </summary>
        public Dictionary<string, MatchResult> MatchLookup { get; }

        /// <summary>
        /// Gets the most recent bracket set for the league.
        /// </summary>
        public BracketSet Bracket { get { return bracketSets.LastOrDefault(); } }

        /// <summary>
        /// Gets the collection of all bracket sets for the league.
        /// </summary>
        public List<BracketSet> Brackets => this.bracketSets;

        /// <summary>
        /// Gets the BracketConfiguration for this bracket.
        /// </summary>
        public BracketConfiguration Configuration => this.configuration;

        /// <summary>
        /// Gets the Teams involved in a league.
        /// </summary>
        public List<Team> Teams { get { return new List<Team>(teams); } }

        /// <summary>
        /// Gets the game id for this bracket.
        /// </summary>
        public string GameId
        {
            get { return this.gameId; }
        }

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
