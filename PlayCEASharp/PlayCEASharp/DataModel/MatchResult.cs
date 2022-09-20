using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayCEASharp.Utilities;

namespace PlayCEASharp.DataModel
{
    public class MatchResult
    {
        internal MatchResult(string matchId) {
            this.MatchId = matchId;
        }

        public override string ToString()
        {
            return this.CustomToString();
        }

        public Team HomeTeam { get; internal set; }

        public Team AwayTeam { get; internal set; }

        public List<Game> Games { get; internal set; }

        public string MatchId { get; private set; }

        public int Round { get; internal set; }

        public int HomeGamesWon { get; internal set; }

        public int AwayGamesWon { get; internal set; }

        public int HomeGoals { get; internal set; }

        public int AwayGoals { get; internal set; }

        public bool Bye { get; internal set; }

        public int HomeGoalDifferential =>
            this.HomeGoals - this.AwayGoals;

        public int AwayGoalDifferential =>
            this.AwayGoals - this.HomeGoals;

        public bool Completed =>
            ((this.HomeGamesWon + this.AwayGamesWon) > 0) || this.Bye;

        public List<Team> Teams { get { return this.Bye ? new List<Team>() { HomeTeam } : new List<Team>() { HomeTeam, AwayTeam }; } }
    }
}
