using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayCEASharp.Configuration;
using PlayCEASharp.Utilities;

namespace PlayCEASharp.DataModel
{
    /// <summary>
    /// A team consists of players. It also has fields for many precomputed stats.
    /// </summary>
    public class Team : ConfiguredNamingObject
    {
        /// <summary>
        /// Creates a new team object with the given team id.
        /// </summary>
        /// <param name="teamId">The team id in playCea.</param>
        public Team(string teamId)
        {
            this.TeamId = teamId;
            this.Stats = new TeamStatistics();
            this.Players = new List<Player>();
            this.RoundStats = new Dictionary<BracketRound, TeamStatistics>();
            this.CumulativeRoundStats = new Dictionary<BracketRound, TeamStatistics>();
            this.StageCumulativeRoundStats = new Dictionary<BracketRound, TeamStatistics>();
            this.StageStats = new Dictionary<string, TeamStatistics>();
            this.RoundRanking = new Dictionary<BracketRound, int>();
            this.FixedRoundRanking = new Dictionary<BracketRound, int>();
        }

        /// <summary>
        /// Resets all of the stats associated with this team.
        /// </summary>
        internal void ResetStats(NamingConfiguration nameConfiguration)
        {
            this.Stats.Reset(nameConfiguration);
            foreach (KeyValuePair<BracketRound, TeamStatistics> pair in this.RoundStats)
            {
                pair.Value.Reset(nameConfiguration);
            }
            foreach (KeyValuePair<BracketRound, TeamStatistics> pair2 in this.CumulativeRoundStats)
            {
                pair2.Value.Reset(nameConfiguration);
            }
            foreach (KeyValuePair<BracketRound, TeamStatistics> pair3 in this.StageCumulativeRoundStats)
            {
                pair3.Value.Reset(nameConfiguration);
            }
            foreach (TeamStatistics statistics in this.StageStats.Values)
            {
                statistics.Reset(nameConfiguration);
            }

            this.RoundRanking.Clear();
            foreach (KeyValuePair<BracketRound, int> kvp in this.FixedRoundRanking)
            {
                this.RoundRanking.Add(kvp.Key, kvp.Value);
            }
        }

        /// <inheritdoc/>
        public override string ToString() => this.CustomToString();

        /// <summary>
        /// The team id in playCea.
        /// </summary>
        public string TeamId { get; }

        /// <summary>
        /// The display name of the team.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// The organization the team is associated with.
        /// </summary>
        public string Org { get; internal set; }

        /// <summary>
        /// The url to the avatar for the team.
        /// </summary>
        public string ImageURL { get; internal set; }

        /// <summary>
        /// The last known rank of this team.
        /// </summary>
        public int Rank { get; internal set; }

        /// <summary>
        /// The players on the team.
        /// </summary>
        public List<Player> Players { get; }

        /// <summary>
        /// The total cumulative statistics for this team.
        /// </summary>
        public TeamStatistics Stats { get; internal set; }

        /// <summary>
        /// Statistics for each specific round.
        /// </summary>
        public Dictionary<BracketRound, TeamStatistics> RoundStats { get; }

        /// <summary>
        /// Statistics that are cumulative up to a specific round.
        /// </summary>
        public Dictionary<BracketRound, TeamStatistics> CumulativeRoundStats { get; }

        /// <summary>
        /// Statistics that are cumulative up to the round, within a given stage.
        /// </summary>
        public Dictionary<BracketRound, TeamStatistics> StageCumulativeRoundStats { get; }

        /// <summary>
        /// The rankings calculated for the team at each round.
        /// </summary>
        public Dictionary<BracketRound, int> RoundRanking { get; }

        /// <summary>
        /// Round rankings which are populated from PlayCEA itself (playoffs).
        /// </summary>
        public Dictionary<BracketRound, int> FixedRoundRanking { get; }

        /// <summary>
        /// The cumulative statistics for each stage.
        /// </summary>
        public Dictionary<string, TeamStatistics> StageStats { get; }
    }
}
