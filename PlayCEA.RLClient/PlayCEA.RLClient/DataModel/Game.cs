using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEA.RLClient.DataModel
{
    public class Game
    {
        public Game(string id)
        {
            this.GameId = id;
        }

        public string ToScoreString() =>
            $"{this.HomeScore}-{this.AwayScore}";

        public override string ToString()
        {
            object[] objArray1 = new object[] { this.GameId, this.HomeTeam, this.AwayTeam, this.HomeScore, this.AwayScore };
            return string.Format("Game Id: {0}, Home Team: {1}, Away Team: {2}, {3}-{4}", (object[])objArray1);
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
