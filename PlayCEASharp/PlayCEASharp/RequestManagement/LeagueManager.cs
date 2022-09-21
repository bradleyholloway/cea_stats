using PlayCEASharp.Analysis;
using PlayCEASharp.Configuration;
using PlayCEASharp.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Backing reference for the current league.
        /// </summary>
        private static League league = null;

        /// <summary>
        /// Locks around refreshing to prevent duplicate work.
        /// </summary>
        private static object refreshLock = new object();

        /// <summary>
        /// The request manager for issuing requests to PlayCEA endpoints.
        /// </summary>
        private static RequestManager rm = new RequestManager();

        /// <summary>
        /// Starts a background refresh thread to update the league periodically.
        /// </summary>
        static LeagueManager()
        {
            Task.Run(new Action(LeagueManager.RefreshThread));
        }

        /// <summary>
        /// Ensures that the league is initialized and popualted.
        /// This is blocking and may take a while.
        /// </summary>
        public static void Bootstrap()
        {
            lock (refreshLock)
            {
                if (league == null)
                {
                    Refresh();
                }
            }
        }

        /// <summary>
        /// Starts the bootstrapping process asyncronously.
        /// </summary>
        /// <returns>Task for bootstrap.</returns>
        public static async Task BootstrapAsync()
        {
            Task bootstrapTask = new Task(Bootstrap);
            await bootstrapTask;
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
                // Read all tournaments.
                List<Tournament> tournaments = rm.GetTournaments().Result;
                // Filter to matching tournaments.
                tournaments = ConfigurationGenerator.MatchingTournaments(tournaments);
                // Populate brackets + teams for the scoped tournaments.
                Task bracketLoading = rm.LoadBrackets(tournaments);
                Task teamLoading = rm.UpdateAllTeams(tournaments.SelectMany(t => t.Teams).Distinct().ToList());
                Task.WaitAll(bracketLoading, teamLoading);
                // Build the stage configurations for analysis from the populated tournaments.
                BracketConfiguration config = ConfigurationGenerator.GenerateConfiguration(tournaments);
                ConfigurationManager.UpdateInMemoryConfiguration(config);
                // Create a local bracket lookup for quick build of bracket sets.
                Dictionary<string, Bracket> bracketLookup = tournaments.SelectMany(t => t.Brackets).ToDictionary(b => b.BracketId);
                // Build bracket sets with actual brackets already populated from initial read.
                List<BracketSet> bracketSets = new List<BracketSet>();
                foreach (string[] brackets in ConfigurationManager.Configuration.bracketSets)
                {
                    bracketSets.Add(new BracketSet(brackets.Select(b => bracketLookup[b]).ToList()));
                }
                // Analyze bracket sets.
                AnalysisManager.Analyze((IEnumerable<BracketSet>)bracketSets);
                // Update league reference.
                league = new League(bracketSets);
            }
        }

        /// <summary>
        /// Background thread to update the league every 5 minutes.
        /// </summary>
        private static void RefreshThread()
        {
            while (true)
            {
                Thread.Sleep(TimeSpan.FromMinutes(5.0));
                Refresh();
            }
        }

        /// <summary>
        /// The current league.
        /// </summary>
        public static League League
        {
            get
            {
                if (league == null)
                {
                    Bootstrap();
                }
                return league;
            }
        }
    }
}
