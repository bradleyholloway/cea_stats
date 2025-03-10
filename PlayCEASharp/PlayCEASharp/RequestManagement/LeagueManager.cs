﻿using PlayCEASharp.Analysis;
using PlayCEASharp.Configuration;
using PlayCEASharp.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static PlayCEASharp.RequestManagement.LeagueInstanceManager;

namespace PlayCEASharp.RequestManagement
{
    /// <summary>
    /// The public entry point to getting the League.
    /// </summary>
    public class LeagueManager
    {
        /// <summary>
        /// Collection of league instance managers.
        /// </summary>
        private static Dictionary<string, LeagueInstanceManager> leagueInstanceManagers;

        /// <summary>
        /// Performance booster to see if bootstrap is already done.
        /// </summary>
        private static bool completedBootstrap = false;

        /// <summary>
        /// Locks around refreshing to prevent duplicate work.
        /// </summary>
        private static object refreshLock = new object();

        /// <summary>
        /// Event handler which can subscribe to be notified when there are new bracket rounds found in any of the leagues.
        /// </summary>
        /// <param name="sender">The object raising the event.</param>
        /// <param name="newRounds">The new rounds which are found.</param>
        public delegate void NewBracketRoundsEventHandler(object sender, Dictionary<BracketRound, League> newRounds);

        /// <summary>
        /// Event handler for when matches are updated. (The number of wins in this cycle does not match the previous number of wins).
        /// </summary>
        /// <param name="sender">The object raising the event.</param>
        /// <param name="updatedMatches">The updated matches.</param>
        public delegate void UpdatedMatchesEventHandler(object sender, Dictionary<MatchResult, League> updatedMatches);

        /// <summary>
        /// Event to subscribe to be notified when there are new BracketRounds located.
        /// </summary>
        public static event NewBracketRoundsEventHandler NewBracketRounds;

        /// <summary>
        /// Event to subscribe to to be notified when there are updated Matches.
        /// </summary>
        public static event UpdatedMatchesEventHandler UpdatedMatches;

        /// <summary>
        /// Way to override the endpoint used by the library.
        /// </summary>
        public static string EndpointOverride = null;

        /// <summary>
        /// Looks up all teams a player is on.
        /// </summary>
        public readonly static Dictionary<string, List<Team>> PlayerLookup = new Dictionary<string, List<Team>>();

        /// <summary>
        /// Looks up all teams a player is on.
        /// </summary>
        public readonly static Dictionary<ulong, List<Team>> PlayerIdLookup = new Dictionary<ulong, List<Team>>();

        /// <summary>
        /// Looks up the upcoming matches for a team, across all tournaments.
        /// </summary>
        public readonly static Dictionary<Team, List<MatchResult>> NextMatchLookup = new Dictionary<Team, List<MatchResult>>();

        /// <summary>
        /// Looks up which leagues a team is part of.
        /// </summary>
        public readonly static Dictionary<Team, List<League>> LeagueLookup = new Dictionary<Team, List<League>>();

        /// <summary>
        /// Looks up a specific MatchResult.
        /// </summary>
        private readonly static Dictionary<string, MatchResult> MatchLookup = new Dictionary<string, MatchResult>();

        /// <summary>
        /// Starts a background refresh thread to update the league periodically.
        /// </summary>
        static LeagueManager()
        {
            Task.Run(LeagueManager.RefreshThread);
        }

        /// <summary>
        /// Ensures that the league is initialized and popualted.
        /// This is blocking and may take a while.
        /// </summary>
        public static void Bootstrap()
        {
            if (completedBootstrap)
            {
                return;
            }

            lock (refreshLock)
            {
                if (leagueInstanceManagers == null)
                {
                    leagueInstanceManagers = new Dictionary<string, LeagueInstanceManager>();
                    Refresh();
                    completedBootstrap = true;
                }
            }
        }

        /// <summary>
        /// Forces a refresh from PlayCEA.
        /// </summary>
        public static void ForceUpdate()
        {
            Refresh();
        }

        /// <summary>
        /// Internally refreshes the league.
        /// </summary>
        private static void Refresh()
        {
            lock (refreshLock)
            {
                TournamentConfigurations tournamentConfigs = ConfigurationManager.TournamentConfigurations;
                foreach (TournamentConfiguration tc in tournamentConfigs.configurations)
                {
                    LeagueInstanceManager instanceManager = leagueInstanceManagers.GetValueOrDefault(tc.id, new LeagueInstanceManager(NewRoundsFound, MatchUpdatesFound, EndpointOverride));
                    instanceManager.ForceUpdate(tc);
                    leagueInstanceManagers[tc.id] = instanceManager;
                }

                // Compute common indicies.
                PlayerLookup.Clear();
                PlayerIdLookup.Clear();
                NextMatchLookup.Clear();
                LeagueLookup.Clear();
                MatchLookup.Clear();

                foreach (LeagueInstanceManager lim in leagueInstanceManagers.Values)
                {
                    League league = lim.League;

                    if (league.Teams.Count > 0)
                    {
                        foreach (KeyValuePair<string, List<Team>> kvp in league.PlayerDiscordLookup)
                        {
                            PlayerLookup[kvp.Key] = PlayerLookup.GetValueOrDefault(kvp.Key, new List<Team>());
                            PlayerLookup[kvp.Key].AddRange(kvp.Value);
                        }

                        foreach (KeyValuePair<ulong, List<Team>> kvp in league.PlayerDiscordIdLookup)
                        {
                            PlayerIdLookup[kvp.Key] = PlayerIdLookup.GetValueOrDefault(kvp.Key, new List<Team>());
                            PlayerIdLookup[kvp.Key].AddRange(kvp.Value);
                        }

                        foreach (Team t in league.Teams)
                        {
                            LeagueLookup[t] = LeagueLookup.GetValueOrDefault(t, new List<League>());
                            LeagueLookup[t].Add(league);
                        }
                    }

                    if (league.Configuration.bracketSets.Length > 0)
                    {
                        foreach (KeyValuePair<Team, MatchResult> kvp in league.NextMatchLookup)
                        {
                            NextMatchLookup[kvp.Key] = NextMatchLookup.GetValueOrDefault(kvp.Key, new List<MatchResult>());
                            NextMatchLookup[kvp.Key].Add(kvp.Value);
                        }

                        foreach (MatchResult m in league.MatchLookup.Values)
                        {
                            MatchLookup[m.MatchId] = m;
                        }
                    }
                }

                // Raise any events needed
                foreach (LeagueInstanceManager lim in leagueInstanceManagers.Values)
                {
                    lim.RaiseEvents(!completedBootstrap);
                }
            }
        }

        private static void NewRoundsFound(object sender, List<BracketRound> newBracketRounds)
        {
            LeagueInstanceManager instanceManager = sender as LeagueInstanceManager;
            NewBracketRounds?.Invoke(sender, newBracketRounds.ToDictionary(r => r, r => instanceManager.League));
        }

        private static void MatchUpdatesFound(object sender, List<MatchResult> updatedMatches)
        {
            LeagueInstanceManager instanceManager = sender as LeagueInstanceManager;
            UpdatedMatches?.Invoke(sender, updatedMatches.ToDictionary(r => r, r => instanceManager.League));
        }

        /// <summary>
        /// Background thread to update the league every 5 minutes.
        /// </summary>
        private static void RefreshThread()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(TimeSpan.FromMinutes(5.0));
                    Refresh();
                }
                catch (Exception e)
                {
                    CeaSharpLogging.Log($"Exception in refresh thread. {e}");
                }
            }
        }

        /// <summary>
        /// The current league.
        /// </summary>
        public static League League
        {
            get
            {
                Bootstrap();
                return leagueInstanceManagers.Values.First().League;
            }
        }

        /// <summary>
        /// All leagues currently watched.
        /// </summary>
        public static List<League> Leagues
        {
            get
            {
                Bootstrap();
                return leagueInstanceManagers.Values.Select(lim => lim.League).ToList();
            }
        }

        /// <summary>
        /// Gets a specific league based on the id from the configuration.
        /// </summary>
        /// <param name="id">The string identifier for the league.</param>
        /// <returns>The league if present. Null if not present.</returns>
        public static League GetLeague(string id)
        {
            Bootstrap();
            if (!leagueInstanceManagers.ContainsKey(id))
            {
                return null;
            }

            return leagueInstanceManagers[id].League;
        }

        /// <summary>
        /// Gets a set of leagues based on the id from the configuration.
        /// </summary>
        /// <param name="idsToExclude">Collection of ids to not include.</param>
        /// <param name="onlyInclude">If any values are present, only include leagues identified here.</param>
        /// <returns>The league if present. Null if not present.</returns>
        public static List<League> GetLeagues(HashSet<string> idsToExclude, HashSet<string> onlyInclude)
        {
            Bootstrap();
            if (onlyInclude == null || onlyInclude.Count == 0)
            {
                return leagueInstanceManagers.Where(lim => !idsToExclude.Contains(lim.Key)).Select(lim => lim.Value.League).ToList();
            }

            return leagueInstanceManagers.Where(lim => onlyInclude.Contains(lim.Key)).Select(lim => lim.Value.League).ToList();
        }

        /// <summary>
        /// Gets a match for a given id, or null if no matches.
        /// </summary>
        /// <param name="MatchId">The unique matchId for the match.</param>
        /// <returns>The MatchResult object.</returns>
        public static MatchResult GetMatch(string MatchId)
        {
            return MatchLookup.GetValueOrDefault(MatchId, null);
        }
    }
}
