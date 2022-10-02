using PlayCEASharp.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayCEASharp.Configuration;

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
        /// <param name="configuration">The bracket configuration to analyze with.</param>
        internal static void Analyze(IEnumerable<BracketSet> brackets, BracketConfiguration configuration)
        {
            foreach (BracketSet set in brackets)
            {
                ResetStats(set, configuration);
                UpdateRoundWeekNumbers(set);
            }
            foreach (BracketSet set2 in brackets)
            {
                BasicStats.CalculateBasicStats(set2, configuration);
                TeamRankAssignmentHelper.PopulateAllRoundRanks(set2, configuration);
            }
        }

        /// <summary>
        /// Resets the stats of all teams in a given bracket.
        /// </summary>
        /// <param name="bracket">Bracket to reset stats for.</param>
        /// <param name="configuration">The BracketConfiguration to analyze with.</param>
        private static void ResetStats(Bracket bracket, BracketConfiguration configuration)
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
                    string key = configuration.StageLookup(round.RoundName);
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
        /// <param name="configuration">The BracketConfiguration to analyze with.</param>
        private static void ResetStats(BracketSet bracketSet, BracketConfiguration configuration)
        {
            foreach (Bracket bracket in bracketSet.Brackets)
            {
                ResetStats(bracket, configuration);
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
