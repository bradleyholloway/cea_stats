
using PlayCEASharp.DataModel;

namespace PlayCEASharp.RequestManagement
{
    /// <summary>
    /// Provides a way to identify if a match is updated.
    /// </summary>
    internal class MatchResultCache
    {
        /// <summary>
        /// The backing cache for the objects.
        /// </summary>
        private readonly Dictionary<string, MatchResult> cache = new Dictionary<string, MatchResult>();

        /// <summary>
        /// Checks if a match has been seen before, and also adds the match to the seen cache.
        /// </summary>
        /// <param name="match">The match to check.</param>
        /// <returns>true if this is a new round.</returns>
        internal bool IsUpdatedMatch(MatchResult match)
        {
            bool hasUpdates = false;
            if (cache.ContainsKey(match.MatchId))
            {
                hasUpdates = HasNewInformation(cache[match.MatchId], match);
            }

            cache[match.MatchId] = new MatchResult(match);
            return hasUpdates;
        }

        internal bool HasNewInformation(MatchResult prev, MatchResult curr)
        {
            return (prev.AwayGamesWon != curr.AwayGamesWon) || (prev.HomeGamesWon != curr.HomeGamesWon);
        }
    }
}
