using PlayCEA.RLClient.Analysis;
using PlayCEA.RLClient.Configuration;
using PlayCEA.RLClient.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEA.RLClient.RequestManagement
{
    public class LeagueManager
    {
        private static League league = null;
        private static List<List<BracketManager>> bracketManagers = null;
        private static BracketConfiguration pastConfiguration = null;
        private static object refreshLock = new object();

        static LeagueManager()
        {
            UpdateFromConfiguration();
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
            UpdateFromConfiguration();
            foreach (BracketManager manager in bracketManagers.SelectMany(b => b))
            {
                if (manager != null)
                {
                    manager.ForceUpdate();
                }
            }
            lock (refreshLock)
            {
                Refresh();
            }
        }

        private static void Refresh()
        {
            List<BracketSet> bracketSets = new List<BracketSet>();
            foreach (List<BracketManager> list2 in bracketManagers)
            {
                bracketSets.Add(new BracketSet(list2.Select(m => m.Bracket).ToList()));
            }
            AnalysisManager.Analyze((IEnumerable<BracketSet>)bracketSets);
            league = new League(bracketSets);
        }

        private static void RefreshThread()
        {
            while (true)
            {
                Thread.Sleep(TimeSpan.FromMinutes(5.0));
                lock (refreshLock)
                {
                    Refresh();
                }
            }
        }

        public static void UpdateFromConfiguration()
        {
            BracketConfiguration configuration = ConfigurationManager.Configuration;
            if ((pastConfiguration != null) && object.ReferenceEquals(pastConfiguration, configuration))
            {
                return;
            }
            else if (bracketManagers != null)
            {
                
                foreach (BracketManager manager in bracketManagers.SelectMany(b => b))
                {
                    manager.Dispose();
                }
            }
            bracketManagers = new List<List<BracketManager>>();
            string[][] bracketSets = configuration.bracketSets;
            int index = 0;
            while (true)
            {
                if (index >= bracketSets.Length)
                {
                    pastConfiguration = configuration;
                    break;
                }
                string[] strArray3 = bracketSets[index];
                bracketManagers.Add(strArray3.Select(id => new BracketManager(id)).ToList());
                index++;
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
