
using PlayCEASharp.Analysis;
using PlayCEASharp.Configuration;
using PlayCEASharp.DataModel;
using PlayCEASharp.RequestManagement;

namespace RlClientTest;

public class Program {
    public static void Main()
    {
        League league = LeagueManager.League;
        
        Console.WriteLine(league);
    }
}

