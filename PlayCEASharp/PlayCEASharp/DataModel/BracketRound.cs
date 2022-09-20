namespace PlayCEASharp.DataModel
{
    public class BracketRound
    {
        internal BracketRound(string roundId)
        {
            this.RoundId = roundId;
            this.Matches = new List<MatchResult>();
        }

        public string RoundId { get; internal set; }

        public List<MatchResult> Matches { get; }

        public List<MatchResult> NonByeMatches
        {
            get
            {
                return this.Matches.Where(m => !m.Bye).ToList();
            }
        }

        public List<MatchResult> ByeMatches
        {
            get
            {
                return this.Matches.Where(m => m.Bye).ToList();
            }
        }

        public string RoundName { get; internal set; }

        public bool Complete { get; internal set; }

        public int GameCount { get; internal set; }

        public int WeekNumber { get; internal set; }

        }
}
