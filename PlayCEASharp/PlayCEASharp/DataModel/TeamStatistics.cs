using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayCEASharp.Utilities;

namespace PlayCEASharp.DataModel
{
    public class TeamStatistics : IComparable
    {
        public TeamStatistics()
        {
            this.Reset();
        }

        public int CompareTo(object obj)
        {
            int num4;
            TeamStatistics statistics = obj as TeamStatistics;
            if (statistics == null)
            {
                throw new NotSupportedException("Cannot compare statistics to nonstatistics object.");
            }
            int num = this.MatchWins - statistics.MatchWins;
            if (num != 0)
            {
                num4 = num;
            }
            else
            {
                int num2 = this.TotalGoalDifferential - statistics.TotalGoalDifferential;
                if (num2 != 0)
                {
                    num4 = num2;
                }
                else
                {
                    int num3 = this.GameDifferential - statistics.GameDifferential;
                    num4 = (num3 == 0) ? 0 : num3;
                }
            }
            return num4;
        }

        public static TeamStatistics operator +(TeamStatistics a, TeamStatistics b)
        {
            TeamStatistics statistics1 = new TeamStatistics();
            statistics1.MatchWins = a.MatchWins + b.MatchWins;
            statistics1.MatchLosses = a.MatchLosses + b.MatchLosses;
            statistics1.GameWins = a.GameWins + b.GameWins;
            statistics1.GameLosses = a.GameLosses + b.GameLosses;
            statistics1.TotalGoals = a.TotalGoals + b.TotalGoals;
            statistics1.TotalGoalsAgainst = a.TotalGoalsAgainst + b.TotalGoalsAgainst;
            return statistics1;
        }

        public static TeamStatistics operator -(TeamStatistics a, TeamStatistics b)
        {
            TeamStatistics statistics1 = new TeamStatistics();
            statistics1.MatchWins = a.MatchWins - b.MatchWins;
            statistics1.MatchLosses = a.MatchLosses - b.MatchLosses;
            statistics1.GameWins = a.GameWins - b.GameWins;
            statistics1.GameLosses = a.GameLosses - b.GameLosses;
            statistics1.TotalGoals = a.TotalGoals - b.TotalGoals;
            statistics1.TotalGoalsAgainst = a.TotalGoalsAgainst - b.TotalGoalsAgainst;
            return statistics1;
        }

        public void Reset()
        {
            this.MatchWins = 0;
            this.MatchLosses = 0;
            this.GameWins = 0;
            this.GameLosses = 0;
            this.TotalGoals = 0;
            this.TotalGoalsAgainst = 0;
        }

        public override string ToString() =>
            this.ToString(false);

        public string ToString(bool detailed)
        {
            return this.CustomToString(detailed);
        }

        public int MatchWins { get; internal set; }

        public int MatchLosses { get; internal set; }

        public int GameWins { get; internal set; }

        public int GameLosses { get; internal set; }

        public int GameDifferential =>
            this.GameWins - this.GameLosses;

        public int TotalGoals { get; internal set; }

        public int TotalGoalsAgainst { get; internal set; }

        public int TotalGames =>
            this.GameWins + this.GameLosses;

        public int TotalGoalDifferential =>
            this.TotalGoals - this.TotalGoalsAgainst;
    }
}
