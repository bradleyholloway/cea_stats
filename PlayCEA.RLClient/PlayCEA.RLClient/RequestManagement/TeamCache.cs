using PlayCEA.RLClient.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEA.RLClient.RequestManagement
{
    public static class TeamCache
    {
        private static readonly Dictionary<string, Team> cache = new Dictionary<string, Team>();

        public static Team GetTeam(string id)
        {
            Team team;
            if (TeamCache.cache.ContainsKey(id))
            {
                team = TeamCache.cache[id];
            }
            else
            {
                Dictionary<string, Team> cache = TeamCache.cache;
                lock (cache)
                {
                    if (TeamCache.cache.ContainsKey(id))
                    {
                        team = TeamCache.cache[id];
                    }
                    else
                    {
                        Team team2 = new Team(id);
                        TeamCache.cache[id] = team2;
                        team = team2;
                    }
                }
            }

            return team;
        }
    }
}
