namespace PlayCEASharp.DataModel
{
    /// <summary>
    /// A BracketRound object contain information for a single round within a bracket.
    /// </summary>
    public class BracketRound
    {
        /// <summary>
        /// Creates a new BracketRound object with the given roundId.
        /// This does not populate any of the other data automatically.
        /// </summary>
        /// <param name="roundId">The roundId in PlayCEA.</param>
        internal BracketRound(string roundId)
        {
            this.RoundId = roundId;
            this.Matches = new List<MatchResult>();
        }

        /// <summary>
        /// Creates a deep copy of the bracket round.
        /// </summary>
        /// <param name="round">The round to copy.</param>
        internal BracketRound(BracketRound round)
        {
            this.RoundId = round.RoundId;
            this.Matches = new List<MatchResult>(round.Matches);
            this.RoundName = round.RoundName;
            this.Complete = round.Complete;
            this.GameCount = round.GameCount;
            this.WeekNumber = round.WeekNumber;
        }

        /// <summary>
        /// The RoundId within PlayCEA.
        /// </summary>
        public string RoundId { get; internal set; }

        /// <summary>
        /// The list of matches for this round.
        /// </summary>
        public List<MatchResult> Matches { get; }

        /// <summary>
        /// A filtered list of matches which are not bye's.
        /// </summary>
        public List<MatchResult> NonByeMatches
        {
            get
            {
                return this.Matches.Where(m => !m.Bye).ToList();
            }
        }

        /// <summary>
        /// A filtered list of matches which are bye's.
        /// </summary>
        public List<MatchResult> ByeMatches
        {
            get
            {
                return this.Matches.Where(m => m.Bye).ToList();
            }
        }

        /// <summary>
        /// The round name in PlayCEA.
        /// </summary>
        public string RoundName { get; internal set; }

        /// <summary>
        /// If the round has been marked as completed in PlayCEA.
        /// </summary>
        public bool Complete { get; internal set; }

        /// <summary>
        /// The number of games for each match in this round.
        /// This is generally the same as "Best of X".
        /// </summary>
        public int GameCount { get; internal set; }

        /// <summary>
        /// The week number for this round.
        /// </summary>
        public int WeekNumber { get; internal set; }

        /// <summary>
        /// The bracket this round belongs to.
        /// </summary>
        public Bracket Bracket { get; internal set; }
    }
}