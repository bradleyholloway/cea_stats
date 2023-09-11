using PlayCEASharp.Analysis;using PlayCEASharp.Configuration;using PlayCEASharp.DataModel;using PlayCEASharp.RequestManagement;using PlayCEASharp.Utilities;
using System.Runtime.CompilerServices;
using System.Text;

namespace RlClientTest;public class Program {    public static void Main()    {
        // LeagueManager.EndpointOverride = "https://z36ny63i72.execute-api.us-east-1.amazonaws.com/staging";
        // TestUpdateMatch();

        // LeagueManager.NewBracketRounds += NewRounds;
        // LeagueManager.UpdatedMatches += UpdatedMatches;
        League league = LeagueManager.League;
        // Console.WriteLine("Done Loading.");

        // Console.WriteLine("Breakpoint before forcerefresh.");
        // LeagueManager.ForceUpdate();

        /*
        // Ad-hoc for getting info on orgs finals performances.
        TournamentConfiguration tc = ConfigurationManager.TournamentConfigurations.configurations.First();
        RequestManager rm = new RequestManager(null);        List<Tournament> tournaments = rm.GetTournaments(tc).Result;        tournaments = tournaments.Where(t => t.SeasonLeague.Equals("CORPORATE") && t.Playoffs != null && !t.Playoffs.BracketId.Equals("")).ToList();        Dictionary<Tournament, MatchResult> finalsMatches = tournaments.ToDictionary(t => t, t => GetFinalsMatch(t, rm, tc));
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
        }        */

        PrintTeamIds();

        // PrintLeagueStats();

        // Prevent ending execution.
        Thread.Sleep(TimeSpan.FromDays(1));    }    private static void PrintTeamIds()
    {
        League league = LeagueManager.League;
        foreach (Team t in league.Bracket.Teams)
        {
            Console.WriteLine(t);
        }
    }    private static void TestUpdateMatch()
    {
        League league = LeagueManager.League;

        MatchResult match = league.NextMatchLookup.Values.First();

        match.Games[0].HomeScore = 2;
        match.Games[0].AwayScore = 0;

        RequestManager rm = new RequestManager(LeagueManager.EndpointOverride);
        rm.ReportScores(match, "bearertoken").Wait();
    }    private static MatchResult GetFinalsMatch(Tournament t, RequestManager rm, TournamentConfiguration tc)
    {
        Bracket playoffBracket = rm.GetBracket(t.Playoffs.BracketId, tc).Result;
        return playoffBracket.Rounds.Last().Matches.Last();
    }    private static void NewRounds(object o, Dictionary<BracketRound, League> newRounds)
    {
        foreach (KeyValuePair<BracketRound, League> r in newRounds)
        {
            Console.WriteLine($"{r.Value.GameId} {r.Key.RoundName} {r.Key.RoundId}");
        }
    }    private static void UpdatedMatches (object o, Dictionary<MatchResult, League> updatedMatches)
    {
        foreach (KeyValuePair<MatchResult, League> m in updatedMatches)
        {
            Console.WriteLine($"{m.Value.GameId} {m.Key.ToString()}");
        }
    }    private static void PrintLeagueStats()
    {
        League league = LeagueManager.League;        List<BracketRound> lastRounds = league.Bracket.Brackets.Select(b => b.Rounds.Last()).ToList();        Dictionary<Team, BracketRound> rLookup = new Dictionary<Team, BracketRound>();        foreach (BracketRound round in lastRounds)
        {
            foreach (Team t in round.Matches.SelectMany(m => m.Teams))
            {
                rLookup[t] = round;
            }
        }        Dictionary<Team, int> rank = league.Bracket.Teams.ToDictionary(t => t, t => t.RoundRanking[rLookup[t]]);        List<Team> teams = league.Bracket.Teams.OrderBy(t => rank[t]).ToList();        string keys = "Team," + teams.First().Stats.ToCSVKeys() + ",tid";        Console.WriteLine(keys);        for (int i = 0; i < teams.Count; i++)
        {
            TeamStatistics stats = teams[i].StageCumulativeRoundStats[rLookup[teams[i]]];
            //Console.WriteLine($"[{i}][{stats.MatchWins}][{stats.TotalGoalDifferential}] {teams[i]}");
            Console.WriteLine($"{teams[i]},{stats.ToCSV()},{teams[i].TeamId}");
        }

        Console.WriteLine();
        Console.WriteLine();

        //Console.WriteLine(GenerateSeedString(teams, 8, 24, 16));
    }    private static string GenerateSeedString(List<Team> teams, int top, int mid, int bottom)
    {
        string topString = string.Join(',', teams.Take(top).Select(t => $"{{ \"S\": \"{t.TeamId}\"}}"));
        string midString = string.Join(',', teams.Skip(top).Take(mid).Select(t => $"{{ \"S\": \"{t.TeamId}\"}}"));
        string botString = string.Join(',', teams.Skip(top + mid).Take(bottom).Select(t => $"{{ \"S\": \"{t.TeamId}\"}}"));


        StringBuilder sb = new StringBuilder();

        sb.Append("\"seed\": { \"M\": { ");
        sb.Append("\"bot\": { \"L\": [");
        sb.Append(botString);
        sb.Append("] },");
        sb.Append("\"mid\": { \"L\": [");
        sb.Append(midString);
        sb.Append("] },");
        sb.Append("\"top\": { \"L\": [");
        sb.Append(topString);
        sb.Append("] }");
        sb.Append("} }");

        return sb.ToString();
    }}