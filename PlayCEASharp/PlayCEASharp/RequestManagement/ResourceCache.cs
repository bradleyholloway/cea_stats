using PlayCEASharp.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEASharp.RequestManagement
{
    internal static class ResourceCache
    {
        private static readonly GenericCache<Team> teamCache = new GenericCache<Team>((id) => new Team(id));

        private static readonly GenericCache<BracketRound> bracketRoundCache = new GenericCache<BracketRound>((id) => new BracketRound(id));

        private static readonly GenericCache<Player> playerCache = new GenericCache<Player>((id) => new Player(id));

        private static readonly GenericCache<Tournament> tournamentCache = new GenericCache<Tournament>((id) => new Tournament(id));

        private static readonly GenericCache<Bracket> bracketCache = new GenericCache<Bracket>((id) => new Bracket(id));

        internal static Team GetTeam(string id)
        {
            return teamCache.Get(id);
        }

        internal static BracketRound GetBracketRound(string id) {
            return bracketRoundCache.Get(id);
        }

        internal static Player GetPlayer(string id) {
            return playerCache.Get(id);
        }

        internal static Tournament GetTournament(string id) {
            return tournamentCache.Get(id);
        }

        internal static Bracket GetBracket(string id) {
            return bracketCache.Get(id);
        }
    }
}
