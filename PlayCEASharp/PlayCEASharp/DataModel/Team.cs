using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayCEASharp.Utilities;

namespace PlayCEASharp.DataModel
{
    public class Team
    {
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
        }

        internal void ResetStats()
        {
            this.Stats.Reset();
            foreach (KeyValuePair<BracketRound, TeamStatistics> pair in this.RoundStats)
            {
                pair.Value.Reset();
            }
            foreach (KeyValuePair<BracketRound, TeamStatistics> pair2 in this.CumulativeRoundStats)
            {
                pair2.Value.Reset();
            }
            foreach (KeyValuePair<BracketRound, TeamStatistics> pair3 in this.StageCumulativeRoundStats)
            {
                pair3.Value.Reset();
            }
            foreach (TeamStatistics statistics in this.StageStats.Values)
            {
                statistics.Reset();
            }
        }

        public override string ToString() => this.CustomToString();

        public string TeamId { get; }

        public string Name { get; internal set; }

        public string Org { get; internal set; }

        public string ImageURL { get; internal set; }

        public int Rank { get; internal set; }

        public List<Player> Players { get; }

        public TeamStatistics Stats { get; internal set; }

        public Dictionary<BracketRound, TeamStatistics> RoundStats { get; }

        public Dictionary<BracketRound, TeamStatistics> CumulativeRoundStats { get; }

        public Dictionary<BracketRound, TeamStatistics> StageCumulativeRoundStats { get; }

        public Dictionary<BracketRound, int> RoundRanking { get; }

        public Dictionary<string, TeamStatistics> StageStats { get; }
    }
}
