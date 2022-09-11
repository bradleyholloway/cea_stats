using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEA.RLClient.DataModel
{
    public class MatchResult
    {
        public override string ToString()
        {
            object[] objArray1 = new object[] { this.MatchId, (int)this.Round, this.HomeTeam, this.AwayTeam, (int)this.HomeGamesWon, (int)this.AwayGamesWon, (int)this.HomeGoalDifferential };
            return string.Format("MatchId: {0}, Round: {1}, Home: {2}, Away:{3}, {4}-{5}, Goal Differential: {6}", (object[])objArray1);
        }

        public Team? HomeTeam { get; set; }

        public Team? AwayTeam { get; set; }

        public List<Game>? Games { get; set; }

        public string? MatchId { get; set; }

        public int Round { get; set; }

        public int HomeGamesWon { get; set; }

        public int AwayGamesWon { get; set; }

        public int HomeGoals { get; set; }

        public int AwayGoals { get; set; }

        public bool Bye { get; set; }

        public int HomeGoalDifferential =>
            this.HomeGoals - this.AwayGoals;

        public int AwayGoalDifferential =>
            this.AwayGoals - this.HomeGoals;

        public bool Completed =>
            ((this.HomeGamesWon + this.AwayGamesWon) > 0) || this.Bye;
    }
}
