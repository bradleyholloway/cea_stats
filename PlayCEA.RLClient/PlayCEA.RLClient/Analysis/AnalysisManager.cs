using PlayCEA.RLClient.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEA.RLClient.Analysis
{
    public static class AnalysisManager
    {
        public static void Analyze(IEnumerable<BracketSet> brackets)
        {
            foreach (BracketSet set in brackets)
            {
                ResetStats(set);
                UpdateRoundWeekNumbers(set);
            }
            foreach (BracketSet set2 in brackets)
            {
                BasicStats.CalculateBasicStats(set2);
                TeamRankAssignmentHelper.PopulateAllRoundRanks(set2);
            }
        }

        private static void ResetStats(Bracket bracket)
        {
            foreach (Team team in bracket.Teams)
            {
                team.ResetStats();
                List<BracketRound> rounds = bracket.Rounds;
                foreach (BracketRound round in rounds)
                {
                    if (!team.RoundStats.ContainsKey(round))
                    {
                        team.RoundStats[round] = new TeamStatistics();
                    }
                    else
                    {
                        team.RoundStats[round].Reset();
                    }
                    if (!team.CumulativeRoundStats.ContainsKey(round))
                    {
                        team.CumulativeRoundStats[round] = new TeamStatistics();
                    }
                    else
                    {
                        team.CumulativeRoundStats[round].Reset();
                    }
                    if (!team.StageCumulativeRoundStats.ContainsKey(round))
                    {
                        team.StageCumulativeRoundStats[round] = new TeamStatistics();
                    }
                    else
                    {
                        team.StageCumulativeRoundStats[round].Reset();
                    }
                    string key = StageMatcher.Lookup(round.RoundName);
                    if (!team.StageStats.ContainsKey(key))
                    {
                        team.StageStats[key] = new TeamStatistics();
                    }
                    else
                    {
                        team.StageStats[key].Reset();
                    }
                }
            }
        }

        private static void ResetStats(BracketSet bracketSet)
        {
            foreach (Bracket bracket in bracketSet.Brackets)
            {
                ResetStats(bracket);
            }
        }

        private static void UpdateRoundWeekNumbers(BracketSet bracket)
        {
            foreach (List<BracketRound> list in bracket.Rounds)
            {
                foreach (BracketRound round in list)
                {
                    round.WeekNumber = round.Matches[0].Round + 1;
                }
            }
        }
    }
}
