using PlayCEASharp.DataModel;
using PlayCEASharp.Utilities;
using System.Text;

namespace CEAClientTest
{
    public class AdminTools
    {
        public static void FindScoreDifferential(League league, int maxDiff)
        {
            foreach (BracketRound round in league.Bracket.Rounds.Last())
            {
                foreach (MatchResult match in round.Matches)
                {
                    if (match.Games != null)
                    {
                        foreach (Game game in match.Games.Where(g => Math.Abs(g.HomeScoreDifferential) > maxDiff))
                        {
                            Console.WriteLine(match);
                        }
                    }
                }
            }
        }

        public static void ReportScoresReminder(League league)
        {
            foreach (BracketRound round in league.Bracket.Rounds.Last())
            {
                foreach (MatchResult match in round.Matches)
                {
                    if (match.Completed == false)
                    {
                        Console.WriteLine($"{match.HomeTeam} vs {match.AwayTeam} <@{match.HomeTeam.Players.Where(p => p.Captain).First().DiscordUID}> <@{match.AwayTeam.Players.Where(p => p.Captain).First().DiscordUID}>");
                    }
                }
            }
        }

        public static void GenerateTeamsOrderString(League league)
        {
            List<BracketRound> lastRounds = league.Bracket.Brackets.Select(b => b.Rounds.Last()).ToList();
            Dictionary<Team, BracketRound> rLookup = new Dictionary<Team, BracketRound>();
            foreach (BracketRound round in lastRounds)
            {
                foreach (Team t in round.Matches.SelectMany(m => m.Teams))
                {
                    rLookup[t] = round;
                }
            }

            List<Team> teams = league.Bracket.Teams.Where(t => rLookup.ContainsKey(t)).ToList();
            Dictionary<Team, int> rank = teams.ToDictionary(t => t, t => t.RoundRanking[rLookup[t]]);
            teams = teams.OrderBy(t => rank[t]).ToList();

            StringBuilder sb = new StringBuilder();

            sb.Append("\"ts\":[");

            foreach (Team t in teams)
            {
                sb.Append("{\"a\":true,\"tid\":\"");
                sb.Append(t.TeamId);
                sb.Append("\"},");
            }

            sb.Append("]");

            Console.WriteLine(sb.ToString());
        }

        public static void GenerateSeedString(League league, int top, int mid, int bottom, bool dynamoFormating = false)
        {
            List<BracketRound> lastRounds = league.Bracket.Brackets.Select(b => b.Rounds.Last()).ToList();
            Dictionary<Team, BracketRound> rLookup = new Dictionary<Team, BracketRound>();
            foreach (BracketRound round in lastRounds)
            {
                foreach (Team t in round.Matches.SelectMany(m => m.Teams))
                {
                    rLookup[t] = round;
                }
            }

            List<Team> teams = league.Bracket.Teams.Where(t => rLookup.ContainsKey(t)).ToList();
            Dictionary<Team, int> rank = teams.ToDictionary(t => t, t => t.RoundRanking[rLookup[t]]);
            teams = teams.OrderBy(t => rank[t]).ToList();

            string idString(string id)
            {
                if (dynamoFormating)
                {
                    return $"{{ \"S\": \"{id}\"}}";
                }
                else
                {
                    return id;
                }
            }

            string topString = $"\"{string.Join("\",\"", teams.Take(top).Select(t => idString(t.TeamId)))}\"";
            string midString = $"\"{string.Join("\",\"", teams.Skip(top).Take(mid).Select(t => idString(t.TeamId)))}\"";
            string botString = $"\"{string.Join("\",\"", teams.Skip(top + mid).Take(bottom).Select(t => idString(t.TeamId)))}\"";

            StringBuilder sb = new StringBuilder();

            sb.Append("\"seed\":{");
            if (dynamoFormating)
            {
                sb.Append("\"M\":{ ");
            }
            sb.Append("\"bot\":");
            if (dynamoFormating)
            {
                sb.Append("{\"L\":");
            }
            sb.Append("[");
            sb.Append(botString);
            sb.Append("]");
            if (dynamoFormating)
            {
                sb.Append("}");
            }
            sb.Append(",\"mid\":");
            if (dynamoFormating)
            {
                sb.Append("{\"L\":");
            }
            sb.Append("[");
            sb.Append(midString);
            sb.Append("]");
            if (dynamoFormating)
            {
                sb.Append("}");
            }
            sb.Append(",\"top\":");
            if (dynamoFormating)
            {
                sb.Append("{\"L\":");
            }
            sb.Append("[");
            sb.Append(topString);
            sb.Append("]");
            if (dynamoFormating)
            {
                sb.Append("}");
            }
            if (dynamoFormating)
            {
                sb.Append("}");
            }
            sb.Append("}");

            Console.WriteLine(sb.ToString());
        }

        public static void PrintLeagueStats(League league)
        {
            List<BracketRound> lastRounds = league.Bracket.Brackets.Select(b => b.Rounds.Last()).ToList();
            Dictionary<Team, BracketRound> rLookup = new Dictionary<Team, BracketRound>();
            foreach (BracketRound round in lastRounds)
            {
                foreach (Team t in round.Matches.SelectMany(m => m.Teams))
                {
                    rLookup[t] = round;
                }
            }

            List<Team> teams = league.Bracket.Teams.Where(t => rLookup.ContainsKey(t)).ToList();
            Dictionary<Team, int> rank = teams.ToDictionary(t => t, t => t.RoundRanking[rLookup[t]]);
            teams = teams.OrderBy(t => rank[t]).ToList();

            string keys = "Team," + teams.First().Stats.ToCSVKeys() + ",Team Id";
            Console.WriteLine(keys);
            for (int i = 0; i < teams.Count; i++)
            {
                TeamStatistics stats = teams[i].StageCumulativeRoundStats[rLookup[teams[i]]];
                Console.WriteLine($"{teams[i]},{stats.ToCSV()},{teams[i].TeamId}");
            }

            Console.WriteLine();
        }

        public static void PrintLeagueTeamIds(League league)
        {
            List<Team> teams = league.Teams.ToList();
            string keys = "Team," + "Team Id";
            Console.WriteLine(keys);
            for (int i = 0; i < teams.Count; i++)
            {
                Console.WriteLine($"{teams[i]},{teams[i].TeamId}");
            }

            Console.WriteLine();
        }

        public static void PrintTeamIds(League league)
        {
            foreach (Team t in league.Teams)
            {
                Console.WriteLine($"{t}, {t.TeamId}");
            }
        }
    }
}
