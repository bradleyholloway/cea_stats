using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayCEASharp.Utilities;

namespace PlayCEASharp.DataModel
{
    /// <summary>
    /// The collection of statistics for a team.
    /// </summary>
    public class TeamStatistics : IComparable
    {
        /// <summary>
        /// Creates a new statistics object.
        /// </summary>
        public TeamStatistics()
        {
            this.Reset();
        }

        /// <inheritdoc/>
        public int CompareTo(object obj)
        {
            int num4;
            TeamStatistics statistics = obj as TeamStatistics;
            if (statistics == null)
            {
                throw new NotSupportedException("Cannot compare statistics to nonstatistics object.");
            }
            int num = this.MatchWins - statistics.MatchWins;
            if (num != 0)
            {
                num4 = num;
            }
            else
            {
                int num2 = this.TotalGoalDifferential - statistics.TotalGoalDifferential;
                if (num2 != 0)
                {
                    num4 = num2;
                }
                else
                {
                    int num3 = this.GameDifferential - statistics.GameDifferential;

                    if (num3 != 0)
                    {
                        num4 = num3;
                    } else
                    {
                        double goalsPerGameDiff = ((double)this.TotalGoals / this.TotalGames) - ((double)statistics.TotalGoals / statistics.TotalGames);
                        if (goalsPerGameDiff > 0)
                        {
                            return 1;
                        } 
                        else if (goalsPerGameDiff < 0)
                        {
                            return -1;
                        } 
                        else
                        {
                            return 0;
                        }
                    }
                }
            }
            return num4;
        }

        /// <summary>
        /// Adds two statistics objects together to create a new statistics object represeting the sum of both.
        /// </summary>
        /// <param name="a">First stats object to add.</param>
        /// <param name="b">Second stats object to add.</param>
        /// <returns>A new TeamStatistics object with the sum of all properties.</returns>
        public static TeamStatistics operator +(TeamStatistics a, TeamStatistics b)
        {
            TeamStatistics statistics1 = new TeamStatistics();
            statistics1.MatchWins = a.MatchWins + b.MatchWins;
            statistics1.MatchLosses = a.MatchLosses + b.MatchLosses;
            statistics1.GameWins = a.GameWins + b.GameWins;
            statistics1.GameLosses = a.GameLosses + b.GameLosses;
            statistics1.TotalGoals = a.TotalGoals + b.TotalGoals;
            statistics1.TotalGoalsAgainst = a.TotalGoalsAgainst + b.TotalGoalsAgainst;
            return statistics1;
        }

        /// <summary>
        /// Subtracts two statistics objects, and returns a new statistics object representing the difference.
        /// </summary>
        /// <param name="a">The first statistics object.</param>
        /// <param name="b">The second statistics object.</param>
        /// <returns>The difference between A and B.</returns>
        public static TeamStatistics operator -(TeamStatistics a, TeamStatistics b)
        {
            TeamStatistics statistics1 = new TeamStatistics();
            statistics1.MatchWins = a.MatchWins - b.MatchWins;
            statistics1.MatchLosses = a.MatchLosses - b.MatchLosses;
            statistics1.GameWins = a.GameWins - b.GameWins;
            statistics1.GameLosses = a.GameLosses - b.GameLosses;
            statistics1.TotalGoals = a.TotalGoals - b.TotalGoals;
            statistics1.TotalGoalsAgainst = a.TotalGoalsAgainst - b.TotalGoalsAgainst;
            return statistics1;
        }

        /// <summary>
        /// Clears all stats for this object.
        /// </summary>
        public void Reset()
        {
            this.MatchWins = 0;
            this.MatchLosses = 0;
            this.GameWins = 0;
            this.GameLosses = 0;
            this.TotalGoals = 0;
            this.TotalGoalsAgainst = 0;
        }

        /// <inheritdoc/>
        public override string ToString() =>
            this.ToString(false);

        /// <summary>
        /// Provides a string representation of the object.
        /// </summary>
        /// <param name="detailed">If more detailed statistics should be included.</param>
        /// <returns>A string representation of the object.</returns>
        public string ToString(bool detailed)
        {
            return this.CustomToString(detailed);
        }

        /// <summary>
        /// The number of matches won.
        /// </summary>
        public int MatchWins { get; internal set; }

        /// <summary>
        /// The number of matches lost.
        /// </summary>
        public int MatchLosses { get; internal set; }

        /// <summary>
        /// The number of games won.
        /// </summary>
        public int GameWins { get; internal set; }

        /// <summary>
        /// The number of games lost.
        /// </summary>
        public int GameLosses { get; internal set; }

        /// <summary>
        /// The difference in games won - games lost.
        /// </summary>
        public int GameDifferential =>
            this.GameWins - this.GameLosses;

        /// <summary>
        /// Total goals scored.
        /// </summary>
        public int TotalGoals { get; internal set; }

        /// <summary>
        /// Total goals against.
        /// </summary>
        public int TotalGoalsAgainst { get; internal set; }

        /// <summary>
        /// The total number of games this object represents.
        /// </summary>
        public int TotalGames =>
            this.GameWins + this.GameLosses;

        /// <summary>
        /// The goal differential of total goals - total goals against.
        /// </summary>
        public int TotalGoalDifferential =>
            this.TotalGoals - this.TotalGoalsAgainst;
    }
}
