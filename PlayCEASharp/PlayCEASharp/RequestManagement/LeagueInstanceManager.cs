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
    internal class LeagueInstanceManager
    {
        /// <summary>
        /// Backing reference for the current league.
        /// </summary>
        private League league = null;

        /// <summary>
        /// The previously used matching configuration for refreshes.
        /// </summary>
        private MatchingConfiguration prevMatchingConfig = null;

        /// <summary>
        /// Locks around refreshing to prevent duplicate work.
        /// </summary>
        private object refreshLock = new object();

        /// <summary>
        /// The request manager for issuing requests to PlayCEA endpoints.
        /// </summary>
        private RequestManager rm = new RequestManager();

        /// <summary>
        /// Ensures that the league is initialized and popualted.
        /// This is blocking and may take a while.
        /// </summary>
        internal void Bootstrap(MatchingConfiguration config)
        {
            lock (refreshLock)
            {
                if (league == null)
                {
                    Refresh(config);
                }
            }
        }

        /// <summary>
        /// Starts the bootstrapping process asyncronously.
        /// </summary>
        /// <returns>Task for bootstrap.</returns>
        internal async Task BootstrapAsync(MatchingConfiguration config)
        {
            Task bootstrapTask = new Task(() => Bootstrap(config));
            await bootstrapTask;
        }

        /// <summary>
        /// Forces a refresh from PlayCEA.
        /// </summary>
        internal void ForceUpdate(MatchingConfiguration config = null)
        {
            Refresh(config ?? prevMatchingConfig);
        }

        /// <summary>
        /// Internally refreshes the league.
        /// </summary>
        private void Refresh(MatchingConfiguration matchingConfig)
        {
            lock (refreshLock)
            {
                // Read all tournaments.
                List<Tournament> tournaments = this.rm.GetTournaments().Result;
                // Filter to matching tournaments.
                tournaments = ConfigurationGenerator.MatchingTournaments(tournaments, matchingConfig);
                // Populate brackets + teams for the scoped tournaments.
                Task bracketLoading = this.rm.LoadBrackets(tournaments);
                Task teamLoading = this.rm.UpdateAllTeams(tournaments.SelectMany(t => t.Teams).Distinct().ToList());
                Task.WaitAll(bracketLoading, teamLoading);
                // Build the stage configurations for analysis from the populated tournaments.
                BracketConfiguration config = ConfigurationGenerator.GenerateConfiguration(tournaments, matchingConfig);
                // Create a local bracket lookup for quick build of bracket sets.
                Dictionary<string, Bracket> bracketLookup = tournaments.SelectMany(t => t.Brackets).ToDictionary(b => b.BracketId);
                // Build bracket sets with actual brackets already populated from initial read.
                List<BracketSet> bracketSets = new List<BracketSet>();
                foreach (string[] brackets in config.bracketSets)
                {
                    bracketSets.Add(new BracketSet(brackets.Select(b => bracketLookup[b]).ToList()));
                }
                // Analyze bracket sets.
                AnalysisManager.Analyze(bracketSets, config);
                // Update league reference.
                this.league = new League(bracketSets, config);
                this.prevMatchingConfig = matchingConfig;
            }
        }

        /// <summary>
        /// The current league.
        /// </summary>
        internal League League
        {
            get
            {
                return league;
            }
        }
    }
}
