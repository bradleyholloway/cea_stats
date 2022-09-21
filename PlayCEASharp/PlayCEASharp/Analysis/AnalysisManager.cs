using PlayCEASharp.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEASharp.Analysis
{
    /// <summary>
    /// Helper class overseeing the analysis for precomputed properties.
    /// </summary>
    internal static class AnalysisManager
    {
        /// <summary>
        /// Analyzes all of the bracket sets, to compute the stats and rankings.
        /// </summary>
        /// <param name="brackets">Ordered collection of brackets to analze.</param>
        internal static void Analyze(IEnumerable<BracketSet> brackets)
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

        /// <summary>
        /// Resets the stats of all teams in a given bracket.
        /// </summary>
        /// <param name="bracket">Bracket to reset stats for.</param>
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

        /// <summary>
        /// Resets stats for all brackets in a bracket set.
        /// </summary>
        /// <param name="bracketSet">The set of brackets to reset stats for.</param>
        private static void ResetStats(BracketSet bracketSet)
        {
            foreach (Bracket bracket in bracketSet.Brackets)
            {
                ResetStats(bracket);
            }
        }

        /// <summary>
        /// Sets the WeekNumber property on rounds in a given bracket.
        /// </summary>
        /// <param name="bracket">The bracket to process.</param>
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
