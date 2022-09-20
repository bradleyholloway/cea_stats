using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayCEASharp.Utilities;

namespace PlayCEASharp.DataModel
{
    public class Game
    {
        internal Game(string id)
        {
            this.GameId = id;
        }

        public override string ToString()
        {
            return this.CustomToString();
        }

        public string GameId { get; }

        public Team HomeTeam { get; internal set; }

        public Team AwayTeam { get; internal set; }

        public int HomeScore { get; internal set; }

        public int AwayScore { get; internal set; }

        public int HomeGoalDifferential =>
            this.HomeScore - this.AwayScore;

        public int AwayGoalDifferential =>
            this.AwayScore - this.HomeScore;
    }
}
