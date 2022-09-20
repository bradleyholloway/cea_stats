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
    public class LeagueManager
    {
        private static League league = null;
        private static object refreshLock = new object();
        private static RequestManager rm = new RequestManager();

        static LeagueManager()
        {
            Task.Run(new Action(LeagueManager.RefreshThread));
        }

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

        public static void ForceUpdate()
        {
            Refresh();
        }

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

        private static void RefreshThread()
        {
            while (true)
            {
                Thread.Sleep(TimeSpan.FromMinutes(5.0));
                Refresh();
            }
        }

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
