using PlayCEA.RLClient.Configuration;
using PlayCEA.RLClient.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEA.RLClient.Analysis
{
    public class TeamRankAssignmentHelper
    {
        private static List<StageGroup> StageGroups;

        static TeamRankAssignmentHelper()
        {
            BuildStageGroups();
        }

        public static void BuildStageGroups()
        {
            StageGroups = Enumerable.ToList<StageGroup>(ConfigurationManager.Configuration.stageGroups);
        }

        public static void PopulateAllRoundRanks(Bracket bracket)
        {
            foreach (BracketRound round in bracket.Rounds)
            {
                PopulateCustomRoundRank(round);
            }
        }

        public static void PopulateAllRoundRanks(BracketSet bracketSet)
        {
            foreach (Bracket bracket in bracketSet.Brackets)
            {
                PopulateAllRoundRanks(bracket);
            }
        }

        public static void PopulateCustomRoundRank(BracketRound round)
        {
            string stage = StageMatcher.Lookup(round.RoundName);
            foreach (StageGroup group in Enumerable.ToList<StageGroup>((IEnumerable<StageGroup>)(from s in (IEnumerable<StageGroup>)StageGroups select s)))
            {
                Func<Team, bool> func4 = delegate(Team t)
                {
                    return t.StageCumulativeRoundStats.ContainsKey(round);
                };
                Func<Team, TeamStatistics> func5 = delegate(Team t) {
                    return t.StageCumulativeRoundStats[round];
                };

                int startingRank = group.StartingRank;
                foreach (Team team in Enumerable.ToList<Team>((IEnumerable<Team>)Enumerable.OrderByDescending<Team, TeamStatistics>((IEnumerable<Team>)Enumerable.ToList<Team>(Enumerable.Where<Team>((IEnumerable<Team>)group.Teams, func4)), func5)))
                {
                    if (!team.RoundRanking.ContainsKey(round))
                    {
                        team.RoundRanking[round] = startingRank;
                    }
                    startingRank++;
                }
            }
        }
    }
}
