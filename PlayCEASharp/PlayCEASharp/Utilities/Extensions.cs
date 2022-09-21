using System;
using PlayCEASharp.Configuration;
using PlayCEASharp.DataModel;

namespace PlayCEASharp.Utilities
{
    /// <summary>
    /// Extensions for the string class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Returns a string with the first character uppercased.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>String with the first character uppercase.</returns>
        /// <exception cref="ArgumentNullException">The input string cannot be null.</exception>
        /// <exception cref="ArgumentException">The input string cannot be empty.</exception>
        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1).ToString())
            };
    }

    /// <summary>
    /// Extensions for the Game class.
    /// </summary>
    public static class GameExtensions
    {
        /// <summary>
        /// Custom ToString using the NamingConfiguration.
        /// </summary>
        /// <param name="input">The Game object.</param>
        /// <returns>String representation of the Game.</returns>
        public static string CustomToString(this Game input)
        {
            object[] objArray1 = new object[] { input.GameId, input.HomeTeam.CustomToString(), input.AwayTeam.CustomToString(), input.HomeScore, input.AwayScore };
            return string.Format("Game Id: {0}, Home: {1}, Away: {2}, {3}-{4}", (object[])objArray1);
        }

        /// <summary>
        /// Gets a string with just the score result of the game.
        /// </summary>
        /// <param name="input">The Game object.</param>
        /// <returns>String with the score.</returns>
        public static string ToScoreString(this Game input) =>
            $"{input.HomeScore}-{input.AwayScore}";
    }

    /// <summary>
    /// Extensions for the Team class.
    /// </summary>
    public static class TeamExtensions
    {
        /// <summary>
        /// Custom to string representation of the team.
        /// </summary>
        /// <param name="input">The team object.</param>
        /// <returns>The string representation of the team.</returns>
        public static string CustomToString(this Team input)
        {
            return (input.Name == null) ? input.TeamId : input.Name;
        }
    }

    /// <summary>
    /// Extensions for the MatchResult class.
    /// </summary>
    public static class MatchResultExtensions
    {
        /// <summary>
        /// Custom ToString for the result of the match.
        /// </summary>
        /// <param name="input">The MatchResult object.</param>
        /// <returns>string representation of the MatchResult.</returns>
        public static string CustomToString(this MatchResult input)
        {
            object[] objArray1 = new object[] { input.MatchId, (int)input.Round, input.HomeTeam.CustomToString(), input.AwayTeam.CustomToString(), (int)input.HomeGamesWon, (int)input.AwayGamesWon, (int)input.HomeGoalDifferential, ConfigurationManager.NamingConfiguration.ScoreWord };
            return string.Format("MatchId:{0}, Round:{1}, Home:{2}, Away:{3}, {4}-{5}, {7} Differential: {6}", (object[])objArray1);
        }
    }

    /// <summary>
    /// Extensions for the Player class.
    /// </summary>
    public static class PlayerExtensions
    {
        /// <summary>
        /// Custom ToString with the player's DiscordId.
        /// </summary>
        /// <param name="input">The Player object.</param>
        /// <returns>String representation of the Player.</returns>
        public static string CustomToString(this Player input)
        {
            return string.Concat(input.Captain ? "(c) " : "", input.DiscordId);
        }
    }

    /// <summary>
    /// Extensions for the TeamStatistics class.
    /// </summary>
    public static class TeamStatisticsExtensions
    {
        /// <summary>
        /// Returns a CSV row of values, according to ToCSVKeys().
        /// </summary>
        /// <param name="input">The TeamStatistics object.</param>
        /// <returns>Comma separated row of values.</returns>
        public static string ToCSV(this TeamStatistics input)
        {
            object[] objArray1 = new object[12];
            objArray1[0] = (int)input.MatchWins;
            objArray1[1] = (int)input.TotalGoalDifferential;
            objArray1[2] = (int)input.GameDifferential;
            objArray1[3] = (int)input.MatchLosses;
            objArray1[4] = input.MatchWins - input.MatchLosses;
            objArray1[5] = (int)input.GameWins;
            objArray1[6] = (int)input.GameLosses;
            objArray1[7] = (int)input.TotalGoals;
            objArray1[8] = (int)input.TotalGoalsAgainst;
            objArray1[9] = (int)input.TotalGames;
            objArray1[10] = ((double)input.TotalGoals) / ((double)input.TotalGames);
            objArray1[11] = ((double)input.TotalGoalsAgainst) / ((double)input.TotalGames);
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}", (object[])objArray1);
        }

        /// <summary>
        /// Gets the CSV keys for using ToCSV on TeamStatistics.
        /// </summary>
        /// <returns>Comma separated list of keys.</returns>
        public static string ToCSVKeys() =>
            $"{ConfigurationManager.NamingConfiguration.MatchWord} Wins," +
            $"{ConfigurationManager.NamingConfiguration.ScoreWord} Diff," +
            $"{ConfigurationManager.NamingConfiguration.GameWord} Diff," +
            $"{ConfigurationManager.NamingConfiguration.MatchWord} Losses," +
            $"{ConfigurationManager.NamingConfiguration.MatchWord} Diff," +
            $"{ConfigurationManager.NamingConfiguration.GameWord} Wins," +
            $"{ConfigurationManager.NamingConfiguration.GameWord} Losses," +
            $"{ConfigurationManager.NamingConfiguration.ScoreWords}," +
            $"{ConfigurationManager.NamingConfiguration.ScoreWords} Against," +
            $"Total {ConfigurationManager.NamingConfiguration.GameWords}," +
            $"{ConfigurationManager.NamingConfiguration.ScoreWords}/{ConfigurationManager.NamingConfiguration.GameWord}," +
            $"{ConfigurationManager.NamingConfiguration.ScoreWords} Against/{ConfigurationManager.NamingConfiguration.GameWord}";

        /// <summary>
        /// CustomToString for TeamStatistics.
        /// </summary>
        /// <param name="input">The TeamStatistics object.</param>
        /// <param name="detailed">If detailed statistics should be included.</param>
        /// <returns>String representation of the TeamStatistics.</returns>
        public static string CustomToString(this TeamStatistics input, bool detailed=false)
        {
            static string f(double d) =>
                ((double)d).ToString("#.000");
            string str = "";
            if (detailed)
            {
                object[] objArray1 = new object[] { input.MatchWins - input.MatchLosses, (int)input.TotalGoalDifferential, (int)input.GameDifferential, f(((double)input.TotalGoals) / ((double)input.TotalGames)), f(((double)input.TotalGoalsAgainst) / ((double)input.TotalGames)),
                    ConfigurationManager.NamingConfiguration.MatchWord, ConfigurationManager.NamingConfiguration.GameWord, ConfigurationManager.NamingConfiguration.ScoreWord, ConfigurationManager.NamingConfiguration.ScoreWords};
                str = string.Format(" {5}Diff: {0}, {7}Diff: {1}, {6}Diff: {2}, {8}/{6}: {3}, {8}Against/{6}: {4}", (object[])objArray1);
            }
            object[] objArray2 = new object[] { (int)input.MatchWins, (int)input.MatchLosses, (int)input.GameWins, (int)input.GameLosses, (int)input.TotalGoals, (int)input.TotalGoalsAgainst, str,
                ConfigurationManager.NamingConfiguration.MatchWords, ConfigurationManager.NamingConfiguration.GameWords, ConfigurationManager.NamingConfiguration.ScoreWords};
            return string.Format("{7} [{0} - {1}], {8} [{2} - {3}], {9} [{4} - {5}]{6}", (object[])objArray2);
        }
    }
}

