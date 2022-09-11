using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEA.RLClient.DataModel
{
    public class TeamStatistics : IComparable
    {
        public TeamStatistics()
        {
            this.Reset();
        }

        public int CompareTo(object? obj)
        {
            int num4;
            TeamStatistics? statistics = obj as TeamStatistics;
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

        public string ToCSV()
        {
            object[] objArray1 = new object[12];
            objArray1[0] = (int)this.MatchWins;
            objArray1[1] = (int)this.TotalGoalDifferential;
            objArray1[2] = (int)this.GameDifferential;
            objArray1[3] = (int)this.MatchLosses;
            objArray1[4] = this.MatchWins - this.MatchLosses;
            objArray1[5] = (int)this.GameWins;
            objArray1[6] = (int)this.GameLosses;
            objArray1[7] = (int)this.TotalGoals;
            objArray1[8] = (int)this.TotalGoalsAgainst;
            objArray1[9] = (int)this.TotalGames;
            objArray1[10] = ((double)this.TotalGoals) / ((double)this.TotalGames);
            objArray1[11] = ((double)this.TotalGoalsAgainst) / ((double)this.TotalGames);
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}", (object[])objArray1);
        }

        public static string ToCSVKeys() =>
            string.Format("Match Wins,Goal Diff,Game Diff,Match Losses,Match Diff,Game Wins,Game Losses,Goals,Goals Against,Total Games,Goals/Game,Goals Against/Game", Array.Empty<object>());

        public override string ToString() =>
            this.ToString(false);

        public string ToString(bool detailed)
        {
            static string f(double d) =>
                ((double)d).ToString("#.000");
            string str = "";
            if (detailed)
            {
                object[] objArray1 = new object[] { this.MatchWins - this.MatchLosses, (int)this.TotalGoalDifferential, (int)this.GameDifferential, f(((double)this.TotalGoals) / ((double)this.TotalGames)), f(((double)this.TotalGoalsAgainst) / ((double)this.TotalGames)) };
                str = string.Format(" MatchDiff: {0}, GoalDiff: {1}, GameDiff: {2}, Goals/Game: {3}, GoalsAgainst/Game: {4}", (object[])objArray1);
            }
            object[] objArray2 = new object[] { (int)this.MatchWins, (int)this.MatchLosses, (int)this.GameWins, (int)this.GameLosses, (int)this.TotalGoals, (int)this.TotalGoalsAgainst, str };
            return string.Format("Matches [{0} - {1}], Games [{2} - {3}], Goals [{4} - {5}]{6}", (object[])objArray2);
        }

        public int MatchWins { get; set; }

        public int MatchLosses { get; set; }

        public int GameWins { get; set; }

        public int GameLosses { get; set; }

        public int GameDifferential =>
            this.GameWins - this.GameLosses;

        public int TotalGoals { get; set; }

        public int TotalGoalsAgainst { get; set; }

        public int TotalGames =>
            this.GameWins + this.GameLosses;

        public int TotalGoalDifferential =>
            this.TotalGoals - this.TotalGoalsAgainst;
    }
}
