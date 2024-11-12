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
    /// Helper class to compute basic statistics for brackets.
    /// </summary>
    internal class BasicStats
    {
        /// <summary>
        /// Calculates the statistics for a bracket.
        /// </summary>
        /// <param name="bracket">The bracket to calculate for.</param>
        /// <param name="config">The BracketConfiguration to use for analysis.</param>
        internal static void CalculateBasicStats(Bracket bracket, BracketConfiguration config)
        {
            BracketRound prevRound = null;
            foreach (BracketRound currentRound in bracket.Rounds)
            {
                string str = config.StageLookup(currentRound.RoundName);
                foreach (MatchResult result in currentRound.NonByeMatches)
                {
                    if (!result.HomeTeam.RoundStats.ContainsKey(currentRound))
                    {
                        result.HomeTeam.RoundStats[currentRound] = new TeamStatistics();
                    }

                    TeamStatistics homeStats = result.HomeTeam.RoundStats[currentRound];
                    homeStats.TotalGoals += result.HomeGoals;
                    TeamStatistics awayStats = result.AwayTeam.RoundStats[currentRound];
                    awayStats.TotalGoals += result.AwayGoals;
                    homeStats.TotalGoalsAgainst += result.AwayGoals;
                    awayStats.TotalGoalsAgainst += result.HomeGoals;
                    homeStats.GameWins += result.HomeGamesWon;
                    homeStats.GameLosses += result.AwayGamesWon;
                    awayStats.GameWins += result.AwayGamesWon;
                    awayStats.GameLosses += result.HomeGamesWon;
                    homeStats.MatchWins += (result.HomeGamesWon > result.AwayGamesWon) ? 1 : 0;
                    homeStats.MatchLosses += (result.AwayGamesWon > result.HomeGamesWon) ? 1 : 0;
                    awayStats.MatchWins += (result.AwayGamesWon > result.HomeGamesWon) ? 1 : 0;
                    awayStats.MatchLosses += (result.HomeGamesWon > result.AwayGamesWon) ? 1 : 0;
                    Team homeTeam = result.HomeTeam;
                    homeTeam.Stats += result.HomeTeam.RoundStats[currentRound];
                    Team awayTeam = result.AwayTeam;
                    awayTeam.Stats += result.AwayTeam.RoundStats[currentRound];
                    Dictionary<string, TeamStatistics> stageStats = result.HomeTeam.StageStats;
                    string key = str;
                    stageStats[key] = stageStats[key] + result.HomeTeam.RoundStats[currentRound];
                    stageStats = result.AwayTeam.StageStats;
                    key = str;
                    stageStats[key] = stageStats[key] + result.AwayTeam.RoundStats[currentRound];
                    result.HomeTeam.CumulativeRoundStats[currentRound] = result.HomeTeam.RoundStats[currentRound];
                    result.AwayTeam.CumulativeRoundStats[currentRound] = result.AwayTeam.RoundStats[currentRound];
                    result.HomeTeam.StageCumulativeRoundStats[currentRound] = result.HomeTeam.RoundStats[currentRound];
                    result.AwayTeam.StageCumulativeRoundStats[currentRound] = result.AwayTeam.RoundStats[currentRound];
                    if (prevRound != null)
                    {
                        Dictionary<BracketRound, TeamStatistics> cumulativeRoundStats = result.HomeTeam.CumulativeRoundStats;
                        cumulativeRoundStats[currentRound] = cumulativeRoundStats[currentRound] + result.HomeTeam.CumulativeRoundStats[prevRound];
                        cumulativeRoundStats = result.AwayTeam.CumulativeRoundStats;
                        cumulativeRoundStats[currentRound] = cumulativeRoundStats[currentRound] + result.AwayTeam.CumulativeRoundStats[prevRound];
                        if (str.Equals(config.StageLookup(currentRound.RoundName)))
                        {
                            cumulativeRoundStats = result.HomeTeam.StageCumulativeRoundStats;
                            cumulativeRoundStats[currentRound] = cumulativeRoundStats[currentRound] + result.HomeTeam.StageCumulativeRoundStats[prevRound];
                            cumulativeRoundStats = result.AwayTeam.StageCumulativeRoundStats;
                            cumulativeRoundStats[currentRound] = cumulativeRoundStats[currentRound] + result.AwayTeam.StageCumulativeRoundStats[prevRound];
                        }
                    }
                }
                foreach (MatchResult result in currentRound.ByeMatches)
                {
                    TeamStatistics local1 = result.HomeTeam.RoundStats[currentRound];
                    local1.TotalGoals += result.HomeGoals;
                    TeamStatistics local9 = result.HomeTeam.RoundStats[currentRound];
                    local9.MatchWins ++;
                    Team homeTeam = result.HomeTeam;
                    homeTeam.Stats += result.HomeTeam.RoundStats[currentRound];
                    Dictionary<string, TeamStatistics> stageStats = result.HomeTeam.StageStats;
                    string key = str;
                    stageStats[key] = stageStats[key] + result.HomeTeam.RoundStats[currentRound];
                    result.HomeTeam.CumulativeRoundStats[currentRound] = result.HomeTeam.RoundStats[currentRound];
                    result.HomeTeam.StageCumulativeRoundStats[currentRound] = result.HomeTeam.RoundStats[currentRound];
                    if (prevRound != null)
                    {
                        Dictionary<BracketRound, TeamStatistics> cumulativeRoundStats = result.HomeTeam.CumulativeRoundStats;
                        BracketRound round3 = currentRound;
                        cumulativeRoundStats[round3] = cumulativeRoundStats[round3] + result.HomeTeam.CumulativeRoundStats[prevRound];
                        if (str.Equals(config.StageLookup(currentRound.RoundName)))
                        {
                            cumulativeRoundStats = result.HomeTeam.StageCumulativeRoundStats;
                            round3 = currentRound;
                            cumulativeRoundStats[round3] = cumulativeRoundStats[round3] + result.HomeTeam.StageCumulativeRoundStats[prevRound];
                        }
                    }
                }

                prevRound = currentRound;
            }
        }

        /// <summary>
        /// Calculates the basic statistics for all brackets in a bracket set.
        /// </summary>
        /// <param name="bracketSet">The bracket set to calculate for.</param>
        /// <param name="configuration">The BracketConfiguration to use for analysis.</param>
        internal static void CalculateBasicStats(BracketSet bracketSet, BracketConfiguration configuration)
        {
            foreach (Bracket bracket in bracketSet.Brackets)
            {
                CalculateBasicStats(bracket, configuration);
            }
        }
    }
}
