using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEA.RLClient.DataModel
{
    public class Bracket
    {
        public Bracket(string bracketId)
        {
            this.BracketId = bracketId;
        }

        public string BracketId { get; }

        public List<BracketRound> Rounds { get; } = new List<BracketRound>();

        public List<Team> Teams { get; } = new List<Team>();

        public string Name { get; set; }
    }
}
