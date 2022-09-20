using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEASharp.DataModel
{
    public class Bracket
    {
        internal Bracket(string bracketId)
        {
            this.BracketId = bracketId;
        }

        public string BracketId { get; }

        public List<BracketRound> Rounds { get; } = new List<BracketRound>();

        public List<Team> Teams { get; } = new List<Team>();

        public string Name { get; internal set; }

        public List<Team> TopMetaTeams { get; internal set; }
        public List<Team> MidMetaTeams { get; internal set; }
        public List<Team> BotMetaTeams { get; internal set; }
    }
}
