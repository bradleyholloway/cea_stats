
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
            else
            {
                hasUpdates = true;
            }

            cache[match.MatchId] = new MatchResult(match);
            return hasUpdates;
        }

        internal bool HasNewInformation(MatchResult prev, MatchResult curr)
        {
            if((prev.AwayGamesWon != curr.AwayGamesWon) || (prev.HomeGamesWon != curr.HomeGamesWon)) {
                return true;
            }

            if (prev.Games == null && curr.Games == null)
            {
                return false;
            }

            if (prev.Games == null || curr.Games == null)
            {
                return true;
            }

            for (int i = 0; i < Math.Min(prev.Games.Count, curr.Games.Count); i++)
            {
                if (prev.Games[i].HomeScore != curr.Games[i].HomeScore || prev.Games[i].AwayScore != curr.Games[i].AwayScore)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
