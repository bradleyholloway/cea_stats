using PlayCEASharp.Configuration;
using PlayCEASharp.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEASharp.Analysis
{
    /// <summary>
    /// Helper class to compute ranks for teams throughout a bracketset.
    /// </summary>
    internal class TeamRankAssignmentHelper
    {
        /// <summary>
        /// Populates all round rankings for a given bracket.
        /// </summary>
        /// <param name="bracket">The bracket to process.</param>
        internal static void PopulateAllRoundRanks(Bracket bracket)
        {
            foreach (BracketRound round in bracket.Rounds)
            {
                PopulateCustomRoundRank(round);
            }
        }

        /// <summary>
        /// Populates all round rankings for a given bracket set.
        /// This requires basic statistics to have already been populated.
        /// </summary>
        /// <param name="bracketSet">The bracket set to process.</param>
        internal static void PopulateAllRoundRanks(BracketSet bracketSet)
        {
            foreach (Bracket bracket in bracketSet.Brackets)
            {
                PopulateAllRoundRanks(bracket);
            }
        }

        /// <summary>
        /// Populates the round rankings for a bracket round.
        /// This requires basic stats to have been populated.
        /// </summary>
        /// <param name="round">The round to process.</param>
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
