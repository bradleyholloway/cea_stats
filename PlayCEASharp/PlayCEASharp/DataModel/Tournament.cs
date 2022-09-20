using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEASharp.DataModel
{
    public class Tournament
    {
        public Tournament(string tournamentId)
        {
            this.TournamentId = tournamentId;
            this.Brackets = new List<Bracket>();
            this.Teams = new List<Team>();
        }

        public string TournamentId { get; }

        public string Message { get; internal set; }

        public object Metadata { get; internal set; }

        public string GameId { get; internal set; }

        public string GameName { get; internal set; }

        public string TournamentName { get; internal set; }

        public bool Live { get; internal set; }

        public bool Current { get; internal set; }

        public List<Bracket> Brackets { get; internal set; }

        public List<Team> Teams { get; internal set; }

        public string SeasonLeague { get; internal set; }

        public string SeasonYear { get; internal set; }

        public string SeasonSeason { get; internal set; }
    }
}
