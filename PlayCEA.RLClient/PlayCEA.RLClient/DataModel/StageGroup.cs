using PlayCEA.RLClient.RequestManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PlayCEA.RLClient.DataModel
{
    public class StageGroup
    {
        [JsonIgnore]
        private HashSet<Team> teams;

        public string[] TeamIds { get; set; }

        public int StartingRank { get; set; }

        public string Stage { get; set; }

        public string Name { get; set; }

        [JsonIgnore]
        public HashSet<Team> Teams
        {
            get
            {
                HashSet<Team> teams;
                if (this.teams != null)
                {
                    teams = this.teams;
                }
                else
                {
                    this.teams = new HashSet<Team>();
                    string[] teamIds = this.TeamIds;
                    int index = 0;
                    while (true)
                    {
                        if (index >= teamIds.Length)
                        {
                            teams = this.teams;
                            break;
                        }
                        string id = teamIds[index];
                        this.teams.Add(TeamCache.GetTeam(id));
                        index++;
                    }
                }
                return teams;
            }
        }
    }
}
