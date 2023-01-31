
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
        private readonly Dictionary<string, DateTime> cache = new Dictionary<string, DateTime>();

        /// <summary>
        /// Checks if a round has been seen before, and also adds the round to the seen cache.
        /// </summary>
        /// <param name="round">The round to check.</param>
        /// <returns>true if this is a new round.</returns>
        internal bool IsNewBracketRound(BracketRound round)
        {
            if (cache.ContainsKey(round.RoundId))
            {
                return false;
            }

            cache.Add(round.RoundId, DateTime.Now);
            return true;
        }
    }
}
