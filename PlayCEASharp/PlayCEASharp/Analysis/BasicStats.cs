﻿using PlayCEASharp.DataModel;
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
        private static void CalculateBasicStats(Bracket bracket, BracketConfiguration config)
        {
            BracketRound round = null;
            foreach (BracketRound round2 in bracket.Rounds)
            {
                string str = config.StageLookup(round2.RoundName);
                foreach (MatchResult result in round2.NonByeMatches)
                {
                    TeamStatistics local1 = result.HomeTeam.RoundStats[round2];
                    local1.TotalGoals += result.HomeGoals;
                    TeamStatistics local2 = result.AwayTeam.RoundStats[round2];
                    local2.TotalGoals += result.AwayGoals;
                    TeamStatistics local3 = result.HomeTeam.RoundStats[round2];
                    local3.TotalGoalsAgainst += result.AwayGoals;
                    TeamStatistics local4 = result.AwayTeam.RoundStats[round2];
                    local4.TotalGoalsAgainst += result.HomeGoals;
                    TeamStatistics local5 = result.HomeTeam.RoundStats[round2];
                    local5.GameWins += result.HomeGamesWon;
                    TeamStatistics local6 = result.HomeTeam.RoundStats[round2];
                    local6.GameLosses += result.AwayGamesWon;
                    TeamStatistics local7 = result.AwayTeam.RoundStats[round2];
                    local7.GameWins += result.AwayGamesWon;
                    TeamStatistics local8 = result.AwayTeam.RoundStats[round2];
                    local8.GameLosses += result.HomeGamesWon;
                    TeamStatistics local9 = result.HomeTeam.RoundStats[round2];
                    local9.MatchWins += (result.HomeGamesWon > result.AwayGamesWon) ? 1 : 0;
                    TeamStatistics local10 = result.HomeTeam.RoundStats[round2];
                    local10.MatchLosses += (result.AwayGamesWon > result.HomeGamesWon) ? 1 : 0;
                    TeamStatistics local11 = result.AwayTeam.RoundStats[round2];
                    local11.MatchWins += (result.AwayGamesWon > result.HomeGamesWon) ? 1 : 0;
                    TeamStatistics local12 = result.AwayTeam.RoundStats[round2];
                    local12.MatchLosses += (result.HomeGamesWon > result.AwayGamesWon) ? 1 : 0;
                    Team homeTeam = result.HomeTeam;
                    homeTeam.Stats += result.HomeTeam.RoundStats[round2];
                    Team awayTeam = result.AwayTeam;
                    awayTeam.Stats += result.AwayTeam.RoundStats[round2];
                    Dictionary<string, TeamStatistics> stageStats = result.HomeTeam.StageStats;
                    string key = str;
                    stageStats[key] = stageStats[key] + result.HomeTeam.RoundStats[round2];
                    stageStats = result.AwayTeam.StageStats;
                    key = str;
                    stageStats[key] = stageStats[key] + result.AwayTeam.RoundStats[round2];
                    result.HomeTeam.CumulativeRoundStats[round2] = result.HomeTeam.RoundStats[round2];
                    result.AwayTeam.CumulativeRoundStats[round2] = result.AwayTeam.RoundStats[round2];
                    result.HomeTeam.StageCumulativeRoundStats[round2] = result.HomeTeam.RoundStats[round2];
                    result.AwayTeam.StageCumulativeRoundStats[round2] = result.AwayTeam.RoundStats[round2];
                    if (round != null)
                    {
                        Dictionary<BracketRound, TeamStatistics> cumulativeRoundStats = result.HomeTeam.CumulativeRoundStats;
                        BracketRound round3 = round2;
                        cumulativeRoundStats[round3] = cumulativeRoundStats[round3] + result.HomeTeam.CumulativeRoundStats[round];
                        cumulativeRoundStats = result.AwayTeam.CumulativeRoundStats;
                        round3 = round2;
                        cumulativeRoundStats[round3] = cumulativeRoundStats[round3] + result.AwayTeam.CumulativeRoundStats[round];
                        if (str.Equals(config.StageLookup(round2.RoundName)))
                        {
                            cumulativeRoundStats = result.HomeTeam.StageCumulativeRoundStats;
                            round3 = round2;
                            cumulativeRoundStats[round3] = cumulativeRoundStats[round3] + result.HomeTeam.StageCumulativeRoundStats[round];
                            cumulativeRoundStats = result.AwayTeam.StageCumulativeRoundStats;
                            round3 = round2;
                            cumulativeRoundStats[round3] = cumulativeRoundStats[round3] + result.AwayTeam.StageCumulativeRoundStats[round];
                        }
                    }
                }
                foreach (MatchResult result in round2.ByeMatches)
                {
                    TeamStatistics local1 = result.HomeTeam.RoundStats[round2];
                    local1.TotalGoals += result.HomeGoals;
                    TeamStatistics local9 = result.HomeTeam.RoundStats[round2];
                    local9.MatchWins ++;
                    Team homeTeam = result.HomeTeam;
                    homeTeam.Stats += result.HomeTeam.RoundStats[round2];
                    Dictionary<string, TeamStatistics> stageStats = result.HomeTeam.StageStats;
                    string key = str;
                    stageStats[key] = stageStats[key] + result.HomeTeam.RoundStats[round2];
                    result.HomeTeam.CumulativeRoundStats[round2] = result.HomeTeam.RoundStats[round2];
                    result.HomeTeam.StageCumulativeRoundStats[round2] = result.HomeTeam.RoundStats[round2];
                    if (round != null)
                    {
                        Dictionary<BracketRound, TeamStatistics> cumulativeRoundStats = result.HomeTeam.CumulativeRoundStats;
                        BracketRound round3 = round2;
                        cumulativeRoundStats[round3] = cumulativeRoundStats[round3] + result.HomeTeam.CumulativeRoundStats[round];
                        if (str.Equals(config.StageLookup(round2.RoundName)))
                        {
                            cumulativeRoundStats = result.HomeTeam.StageCumulativeRoundStats;
                            round3 = round2;
                            cumulativeRoundStats[round3] = cumulativeRoundStats[round3] + result.HomeTeam.StageCumulativeRoundStats[round];
                        }
                    }
                }

                round = round2;
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
