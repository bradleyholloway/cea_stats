
using PlayCEASharp.DataModel;

namespace PlayCEASharp.RequestManagement
{
    /// <summary>
    /// Provides a way to identify if a bracket round is new or previously seen.
    /// </summary>
    internal class BracketRoundCache
    {
        /// <summary>
        /// The backing cache for the objects.
        /// </summary>
        private readonly Dictionary<string, BracketRound> cache = new Dictionary<string, BracketRound>();

        /// <summary>
        /// Checks if a round has been seen before, and also adds the round to the seen cache.
        /// </summary>
        /// <param name="round">The round to check.</param>
        /// <returns>true if this is a new round.</returns>
        internal bool IsNewBracketRound(BracketRound round)
        {
            bool hasUpdates = false;
            if (cache.ContainsKey(round.RoundId))
            {
                hasUpdates = HasNewInformation(cache[round.RoundId], round);
            }

            cache.Add(round.RoundId, new BracketRound(round));
            return hasUpdates;
        }

        internal bool HasNewInformation(BracketRound prev, BracketRound curr)
        {
            return curr.Matches.Count != prev.Matches.Count;
        }
    }
}
