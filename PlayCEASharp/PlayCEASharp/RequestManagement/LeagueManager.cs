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
        /// Backing reference for the current league.
        /// </summary>
        private static LeagueInstanceManager leagueInstanceManager = null;

        /// <summary>
        /// Locks around refreshing to prevent duplicate work.
        /// </summary>
        private static object refreshLock = new object();

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
            lock (refreshLock)
            {
                if (leagueInstanceManager == null)
                {
                    leagueInstanceManager = new LeagueInstanceManager();
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
                // Eventually we should be able to handle multiple configurations within the library.
                // LeagueManager will need to manage different LeagueInstanceManagers and behave appropriately.
                // For now only one instance is managed.
                MatchingConfiguration matchingConfig = ConfigurationManager.MatchingConfiguration;
                leagueInstanceManager.ForceUpdate(matchingConfig);
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
                    Logger.Log($"Exception in refresh thread. {e.ToString()}");
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
                if (leagueInstanceManager == null)
                {
                    Bootstrap();
                }
                return leagueInstanceManager.League;
            }
        }
    }
}
