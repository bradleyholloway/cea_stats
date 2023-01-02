using PlayCEASharp.Analysis;using PlayCEASharp.Configuration;using PlayCEASharp.DataModel;using PlayCEASharp.RequestManagement;using PlayCEASharp.Utilities;

namespace RlClientTest;public class Program {    public static void Main()    {        League league = LeagueManager.League;        List<BracketRound> lastRounds = league.Bracket.Brackets.Select(b => b.Rounds.Last()).ToList();        Dictionary<Team, BracketRound> rLookup = new Dictionary<Team, BracketRound>();        foreach (BracketRound round in lastRounds)
        {
            foreach (Team t in round.Matches.SelectMany(m => m.Teams))
            {
                rLookup[t] = round;
            }
        }        Dictionary<Team, int> rank = league.Bracket.Teams.ToDictionary(t => t, t => t.RoundRanking[rLookup[t]]);        List<Team> teams = league.Bracket.Teams.OrderBy(t => rank[t]).ToList();        string keys = "Team," + TeamStatisticsExtensions.ToCSVKeys();        Console.WriteLine(keys);        for (int i = 0; i < teams.Count; i++)
        {
            TeamStatistics stats = teams[i].StageCumulativeRoundStats[rLookup[teams[i]]];
            //Console.WriteLine($"[{i}][{stats.MatchWins}][{stats.TotalGoalDifferential}] {teams[i]}");
            Console.WriteLine($"{teams[i]},{stats.ToCSV()}");
        }        Console.WriteLine(league);    }}