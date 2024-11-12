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
        /// <param name="nameConfiguration">The naming configuration used for this bracket.</param>
        internal static void Analyze(IEnumerable<BracketSet> brackets, BracketConfiguration configuration, NamingConfiguration nameConfiguration)
        {
            foreach (BracketSet set in brackets)
            {
                ResetStats(set, configuration, nameConfiguration);
                UpdateRoundWeekNumbers(set);
            }
            foreach (BracketSet set in brackets)
            {
                BasicStats.CalculateBasicStats(set, configuration);
                TeamRankAssignmentHelper.PopulateAllRoundRanks(set, configuration);
            }
        }

        /// <summary>
        /// Resets the stats of all teams in a given bracket.
        /// </summary>
        /// <param name="bracket">Bracket to reset stats for.</param>
        /// <param name="configuration">The BracketConfiguration to analyze with.</param>
        /// <param name="nameConfiguration">The NamingConfiguration used for this bracket.</param>
        internal static void ResetStats(Bracket bracket, BracketConfiguration configuration, NamingConfiguration nameConfiguration)
        {
            HashSet<Team> resetTeams = new HashSet<Team>(bracket.Teams);
            foreach (Team team in bracket.Teams)
            {
                team.ResetStats(nameConfiguration);
                List<BracketRound> rounds = bracket.Rounds;
                foreach (BracketRound round in rounds)
                {
                    ResetStatsForTeamRound(team, round, nameConfiguration, configuration);
                }
            }

            foreach (BracketRound round in bracket.Rounds)
            {
                foreach (MatchResult match in round.Matches)
                {
                    if (!resetTeams.Contains(match.HomeTeam))
                    {
                        ResetStatsForTeamRound(match.HomeTeam, round, nameConfiguration, configuration);
                    }

                    if (!match.Bye && !resetTeams.Contains(match.AwayTeam))
                    {
                        ResetStatsForTeamRound(match.AwayTeam, round, nameConfiguration, configuration);
                    }
                }
            }
        }

        private static void ResetStatsForTeamRound(Team team, BracketRound round, NamingConfiguration nameConfiguration, BracketConfiguration configuration)
        {
            if (!team.RoundStats.ContainsKey(round))
            {
                team.RoundStats[round] = new TeamStatistics()
                {
                    NameConfiguration = nameConfiguration
                };
            }
            else
            {
                team.RoundStats[round].Reset(nameConfiguration);
            }
            if (!team.CumulativeRoundStats.ContainsKey(round))
            {
                team.CumulativeRoundStats[round] = new TeamStatistics()
                {
                    NameConfiguration = nameConfiguration
                };
            }
            else
            {
                team.CumulativeRoundStats[round].Reset(nameConfiguration);
            }
            if (!team.StageCumulativeRoundStats.ContainsKey(round))
            {
                team.StageCumulativeRoundStats[round] = new TeamStatistics()
                {
                    NameConfiguration = nameConfiguration
                };
            }
            else
            {
                team.StageCumulativeRoundStats[round].Reset(nameConfiguration);
            }
            string key = configuration.StageLookup(round.RoundName);
            if (!team.StageStats.ContainsKey(key))
            {
                team.StageStats[key] = new TeamStatistics()
                {
                    NameConfiguration = nameConfiguration
                };

            }
            else
            {
                team.StageStats[key].Reset(nameConfiguration);
            }
        }

        /// <summary>
        /// Resets stats for all brackets in a bracket set.
        /// </summary>
        /// <param name="bracketSet">The set of brackets to reset stats for.</param>
        /// <param name="configuration">The BracketConfiguration to analyze with.</param>
        /// <param name="nameConfiguration">The naming configuration for this bracket.</param>
        private static void ResetStats(BracketSet bracketSet, BracketConfiguration configuration, NamingConfiguration nameConfiguration)
        {
            foreach (Bracket bracket in bracketSet.Brackets)
            {
                ResetStats(bracket, configuration, nameConfiguration);
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
