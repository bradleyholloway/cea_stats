using PlayCEASharp.Analysis;using PlayCEASharp.Configuration;using PlayCEASharp.DataModel;using PlayCEASharp.RequestManagement;using PlayCEASharp.Utilities;

namespace RlClientTest;public class Program {    public static void Main()    {
        League league = LeagueManager.League;
        Console.WriteLine("Done.");

        /*        League league = LeagueManager.League;        List<BracketRound> lastRounds = league.Bracket.Brackets.Select(b => b.Rounds.Last()).ToList();        Dictionary<Team, BracketRound> rLookup = new Dictionary<Team, BracketRound>();        foreach (BracketRound round in lastRounds)
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
  }        Console.WriteLine(league);        */

        RequestManager rm = new RequestManager();        List<Tournament> tournaments = rm.GetTournaments().Result;        tournaments = tournaments.Where(t => t.SeasonLeague.Equals("CORPORATE") && t.Playoffs != null && !t.Playoffs.BracketId.Equals("")).ToList();        Dictionary<Tournament, MatchResult> finalsMatches = tournaments.ToDictionary(t => t, t => GetFinalsMatch(t, rm));
        Dictionary<string, int> wins = new Dictionary<string, int>();
        Dictionary<string, int> finals = new Dictionary<string, int>();
        foreach (KeyValuePair<Tournament, MatchResult> result in finalsMatches)
        {
            Team winner = (result.Value.HomeGamesWon > result.Value.AwayGamesWon) ? result.Value.HomeTeam : result.Value.AwayTeam;
            Team loser = (result.Value.HomeGamesWon > result.Value.AwayGamesWon) ? result.Value.AwayTeam : result.Value.HomeTeam;
            wins[winner.Org] = wins.GetValueOrDefault(winner.Org) + 1;
            finals[winner.Org] = finals.GetValueOrDefault(winner.Org) + 1;
            finals[loser.Org] = finals.GetValueOrDefault(loser.Org) + 1;
            Console.WriteLine($"{result.Value.HomeGamesWon == result.Value.AwayGamesWon}--{winner.Org},{loser.Org},{winner.Name},{loser.Name},{result.Key.GameName},{result.Key.GameId},{result.Key.TournamentName},{result.Key.SeasonYear},{result.Key.SeasonSeason}");
        }        foreach (string org in finals.Keys)
        {
            int win = wins.GetValueOrDefault(org);
            //Console.WriteLine($"{org} {win} {finals[org]}");
        }    }    private static MatchResult GetFinalsMatch(Tournament t, RequestManager rm)
    {
        Bracket playoffBracket = rm.GetBracket(t.Playoffs.BracketId).Result;
        return playoffBracket.Rounds.Last().Matches.Last();
    }}