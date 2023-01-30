using PlayCEASharp.Analysis;
using PlayCEASharp.Configuration;
using PlayCEASharp.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
        /// Looks up all teams a player is on.
        /// </summary>
        public readonly static Dictionary<string, List<Team>> PlayerLookup = new Dictionary<string, List<Team>>();

        /// <summary>
        /// Looks up the upcoming matches for a team, across all tournaments.
        /// </summary>
        public readonly static Dictionary<Team, List<MatchResult>> NextMatchLookup = new Dictionary<Team, List<MatchResult>>();

        /// <summary>
        /// Looks up which leagues a team is part of.
        /// </summary>
        public readonly static Dictionary<Team, List<League>> LeagueLookup = new Dictionary<Team, List<League>>();

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
                    LeagueInstanceManager instanceManager = leagueInstanceManagers.GetValueOrDefault(tc.id, new LeagueInstanceManager());
                    instanceManager.ForceUpdate(tc);
                    leagueInstanceManagers[tc.id] = instanceManager;
                }

                // Compute common indicies.
                PlayerLookup.Clear();
                NextMatchLookup.Clear();
                LeagueLookup.Clear();
                foreach (LeagueInstanceManager lim in leagueInstanceManagers.Values)
                {
                    League league = lim.League;
                    foreach (KeyValuePair<string, List<Team>> kvp in league.PlayerDiscordLookup)
                    {
                        PlayerLookup[kvp.Key] = PlayerLookup.GetValueOrDefault(kvp.Key, new List<Team>());
                        PlayerLookup[kvp.Key].AddRange(kvp.Value);
                    }

                    foreach (KeyValuePair<Team, MatchResult> kvp in league.NextMatchLookup)
                    {
                        NextMatchLookup[kvp.Key] = NextMatchLookup.GetValueOrDefault(kvp.Key, new List<MatchResult>());
                        NextMatchLookup[kvp.Key].Add(kvp.Value);
                    }

                    foreach (Team t in league.Bracket.Teams)
                    {
                        LeagueLookup[t] = LeagueLookup.GetValueOrDefault(t, new List<League>());
                        LeagueLookup[t].Add(League);
                    }
                }
            }
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

        public static League GetLeague(string id)
        {
            Bootstrap();
            if (!leagueInstanceManagers.ContainsKey(id))
            {
                return null;
            }

            return leagueInstanceManagers[id].League;
        }
    }
}
