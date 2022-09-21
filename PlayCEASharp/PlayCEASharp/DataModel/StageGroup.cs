using PlayCEASharp.RequestManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PlayCEASharp.DataModel
{
    /// <summary>
    /// A StageGroup is a collection of teams which are grouped together for a given stage.
    /// </summary>
    public class StageGroup
    {
        /// <summary>
        /// The collection of teams in this stage group.
        /// </summary>
        [JsonIgnore]
        private HashSet<Team> teams;

        /// <summary>
        /// The collection of team ids in this group.
        /// </summary>
        public string[] TeamIds { get; set; }

        /// <summary>
        /// The starting rank of the group.
        /// </summary>
        public int StartingRank { get; set; }

        /// <summary>
        /// The stage this group is for.
        /// </summary>
        public string Stage { get; set; }

        /// <summary>
        /// The display name of this stage group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The collection of teams in this stage group.
        /// </summary>
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
                        this.teams.Add(ResourceCache.GetTeam(id));
                        index++;
                    }
                }
                return teams;
            }
        }
    }
}
