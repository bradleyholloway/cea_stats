using CEAClientTest;
using PlayCEASharp.Analysis;using PlayCEASharp.Configuration;using PlayCEASharp.DataModel;using PlayCEASharp.RequestManagement;using PlayCEASharp.Utilities;
using System.Runtime.CompilerServices;
using System.Text;

namespace RlClientTest;public class Program {    const string StagingEndpoint = "https://z36ny63i72.execute-api.us-east-1.amazonaws.com/staging";
    
    public static void Main()    {

        // LeagueManager.EndpointOverride = StagingEndpoint;
        // League league = LeagueManager.League;

        RequestManager rm = new RequestManager(null);
        Team adobe = rm.GetTeam("NbHbj4gmbk", null).Result;

        //AdminTools.FindScoreDifferential(league, 4);
        //AdminTools.ReportScoresReminder(league);

        //AdminTools.PrintLeagueTeamIds(league);
        // AdminTools.PrintLeagueStats(league);

        /*
        string[][] teamsToCreate = new string[][]
        {
            new string[]{ "IND Lords Of CS", "Aristocrat Labs", ""},

        };

        foreach (string[] teamToCreate in teamsToCreate)
        {
            Team createdTeam = rm
                .CreateTeam(teamToCreate[0], teamToCreate[1], teamToCreate[2], bearerToken)
                .Result;
            Console.WriteLine($"{createdTeam.Name} {createdTeam.TeamId}");
            Console.WriteLine(rm.AddTeamToTournament(createdTeam.TeamId, "tournamentid", bearerToken).Result);
            Console.WriteLine($"{createdTeam} NewCode: {rm.GenerateNewInviteCode(createdTeam.TeamId, bearerToken).Result}");
        }
        */

        /*
        string tournamentId = "";
        string[] teamsToAdd = new string[] { "" };

        foreach (string team in teamsToAdd)
        {
            Console.WriteLine(rm.AddTeamToTournament(team, tournamentId, bearerToken).Result);
        }
        */

        //MatchResult m1 = rm.GetMatchResult("UmrNa2CdUO").Result;
        //MatchResult m2 = rm.GetMatchResult("9Vn0WXAdeq").Result;

        /*
        TournamentConfiguration tc = new TournamentConfiguration()
        {
            namingConfig = new NamingConfiguration() { },
        };
        List<Tournament> allTournaments = rm.GetTournaments().Result;
        allTournaments = allTournaments.Where(t => t.GameId == "rl" && t.Playoffs != null).ToList();
        List<Bracket> playoffsBrackets = allTournaments.Select(t => rm.GetBracket(t.Playoffs.BracketId, tc).Result).ToList();
        foreach (Tournament t in allTournaments)
        {
            PrintStatsForPlayoffs(t, tc.namingConfig);
        }
        */

        // AdminTools.GenerateSeedString(league, 6, 11, 8);
        // AdminTools.GenerateTeamsOrderString(league);

        // Prevent ending execution.
        Thread.Sleep(TimeSpan.FromDays(1));    }    private static void PrintStatsForPlayoffs(Tournament t, NamingConfiguration nc)
    {
        if (t.Playoffs == null)
        {
            return;
        }

        Console.WriteLine($"{t.TournamentName} {t.SeasonYear} {t.SeasonSeason}");
        var bc = new BracketConfiguration();
        AnalysisManager.ResetStats(t.Playoffs, bc, nc);
        BasicStats.CalculateBasicStats(t.Playoffs, bc);
        List<Team> rankedTeams = t.Playoffs.Teams.OrderByDescending(t => t.Stats).ToList();
        Console.WriteLine($"Name,Rank,{rankedTeams.First().Stats.ToCSVKeys()}");
        int i = 1;
        foreach (Team team in rankedTeams) {
            Console.WriteLine($"{team.Name.Replace(',', '.')},{i++},{team.Stats.ToCSV()}");
        }

        Console.WriteLine();


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
    }}