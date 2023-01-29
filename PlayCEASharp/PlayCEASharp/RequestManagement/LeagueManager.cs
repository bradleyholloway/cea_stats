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
                TournamentConfigurations tournamentConfigs = ConfigurationManager.TournamentConfigurations;
                foreach (TournamentConfiguration tc in tournamentConfigs.configurations)
                {
                    LeagueInstanceManager instanceManager = leagueInstanceManagers.GetValueOrDefault(tc.id, new LeagueInstanceManager());
                    instanceManager.ForceUpdate(tc);
                    leagueInstanceManagers[tc.id] = instanceManager;
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

        public static League GetLeague(string id)
        {
            Bootstrap();
            return leagueInstanceManagers[id].League;
        }
    }
}
