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
            foreach (StageGroup group in StageGroups)
            {
                int startingRank = group.StartingRank;
                foreach (Team team in group.Teams.Where(t => t.StageCumulativeRoundStats.ContainsKey(round)).OrderBy(t => t.StageCumulativeRoundStats[round]))
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
