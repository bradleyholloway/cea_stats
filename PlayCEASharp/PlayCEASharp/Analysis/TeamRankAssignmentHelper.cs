using PlayCEASharp.Configuration;
using PlayCEASharp.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEASharp.Analysis
{
    internal class TeamRankAssignmentHelper
    {
        internal static void PopulateAllRoundRanks(Bracket bracket)
        {
            foreach (BracketRound round in bracket.Rounds)
            {
                PopulateCustomRoundRank(round);
            }
        }

        internal static void PopulateAllRoundRanks(BracketSet bracketSet)
        {
            foreach (Bracket bracket in bracketSet.Brackets)
            {
                PopulateAllRoundRanks(bracket);
            }
        }

        internal static void PopulateCustomRoundRank(BracketRound round)
        {
            string stage = StageMatcher.Lookup(round.RoundName);
            foreach (StageGroup group in ConfigurationManager.Configuration.stageGroups.Where(s => s.Stage.Equals(stage)))
            {
                Func<Team, bool> func4 = delegate(Team t)
                {
                    return t.StageCumulativeRoundStats.ContainsKey(round);
                };
                Func<Team, TeamStatistics> func5 = delegate(Team t) {
                    return t.StageCumulativeRoundStats[round];
                };

                int startingRank = group.StartingRank;
                HashSet<Team> teamsInRound = round.Matches.SelectMany(m => m.Teams).ToHashSet();
                
                foreach (Team team in group.Teams.Where(func4).OrderByDescending(func5).ToList())
                {
                    if (!team.RoundRanking.ContainsKey(round))
                    {
                        if (!teamsInRound.Contains(team))
                        {
                            team.RoundRanking[round] = 999;
                        }
                        else
                        {
                            team.RoundRanking[round] = startingRank;
                        }
                    }

                    startingRank++;
                }
            }
        }
    }
}
