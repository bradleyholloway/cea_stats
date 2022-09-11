

using PlayCEA.RLClient.DataModel;
using PlayCEA.RLClient.RequestManagement;

namespace RlClientTest;

public class Program {
    public static void Main()
    {
        League league = LeagueManager.League;

        Console.WriteLine(league);
    }
}

