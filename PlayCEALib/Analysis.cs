namespace PlayCEALib;

/// <summary>
/// A local standing derived from the bracket graph. The API can provide
/// richer standings later without changing the client-facing shape.
/// </summary>
public sealed class BracketStanding
{
    public long EntryId { get; init; }
    public string? EntryName { get; init; }
    public int MatchesPlayed { get; init; }
    public int Wins { get; init; }
    public int Losses { get; init; }
    public int GamesWon { get; init; }
    public int GamesLost { get; init; }
    public int Forfeits { get; init; }
}

public static class BracketAnalysis
{
    public static IReadOnlyList<BracketStanding> CalculateStandings(BracketGraph graph)
    {
        var totals = new Dictionary<long, MutableStanding>();

        foreach (var node in graph.Nodes)
        {
            if (!IsCompleted(node.State) || node.Slots.Count == 0)
            {
                continue;
            }

            var slots = node.Slots
                .Where(slot => slot.EntryId.HasValue)
                .ToList();
            if (slots.Count == 0)
            {
                continue;
            }

            var highestGamesWon = slots.Max(slot => slot.GamesWon ?? 0);
            var winners = slots
                .Where(slot => (slot.GamesWon ?? 0) == highestGamesWon)
                .ToList();
            var winnerId = winners.Count == 1 ? winners[0].EntryId : null;

            foreach (var slot in slots)
            {
                var entryId = slot.EntryId!.Value;
                if (!totals.TryGetValue(entryId, out var standing))
                {
                    standing = new MutableStanding(entryId, slot.EntryName);
                    totals.Add(entryId, standing);
                }

                standing.MatchesPlayed++;
                standing.GamesWon += slot.GamesWon ?? 0;
                standing.Forfeits += slot.Forfeited ? 1 : 0;
                if (winnerId == entryId)
                {
                    standing.Wins++;
                }
                else if (winnerId.HasValue)
                {
                    standing.Losses++;
                }
            }

            foreach (var slot in slots)
            {
                if (slot.EntryId.HasValue)
                {
                    totals[slot.EntryId.Value].GamesLost += slots
                        .Where(other => other.EntryId != slot.EntryId)
                        .Sum(other => other.GamesWon ?? 0);
                }
            }
        }

        return totals.Values
            .Select(standing => standing.ToResult())
            .OrderByDescending(standing => standing.Wins)
            .ThenByDescending(standing => standing.GamesWon)
            .ThenBy(standing => standing.EntryName)
            .ToList();
    }

    private static bool IsCompleted(string? state) =>
        state != null &&
        (state.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase) ||
         state.Equals("COMPLETE", StringComparison.OrdinalIgnoreCase) ||
         state.Equals("FINISHED", StringComparison.OrdinalIgnoreCase) ||
         state.Equals("CLOSED", StringComparison.OrdinalIgnoreCase));

    private sealed class MutableStanding
    {
        public MutableStanding(long entryId, string? entryName)
        {
            EntryId = entryId;
            EntryName = entryName;
        }

        public long EntryId { get; }
        public string? EntryName { get; }
        public int MatchesPlayed { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int GamesWon { get; set; }
        public int GamesLost { get; set; }
        public int Forfeits { get; set; }

        public BracketStanding ToResult() => new()
        {
            EntryId = EntryId,
            EntryName = EntryName,
            MatchesPlayed = MatchesPlayed,
            Wins = Wins,
            Losses = Losses,
            GamesWon = GamesWon,
            GamesLost = GamesLost,
            Forfeits = Forfeits
        };
    }
}

/// <summary>
/// Detects changes between successive match reads, analogous to the legacy
/// MatchResultCache but independent of the legacy model types.
/// </summary>
public sealed class MatchChangeTracker
{
    private readonly Dictionary<long, string> snapshots = new();

    public bool HasChanged(CompetitionMatch match)
    {
        var signature = BuildSignature(match);
        var changed = !snapshots.TryGetValue(match.Id, out var previous) ||
                      !String.Equals(previous, signature, StringComparison.Ordinal);
        snapshots[match.Id] = signature;
        return changed;
    }

    private static string BuildSignature(CompetitionMatch match)
    {
        var slots = String.Join("|", match.Slots
            .OrderBy(slot => slot.Position)
            .Select(slot =>
                $"{slot.Position}:{slot.Assignment?.Id}:{slot.RoundsScored}:{slot.GamesWon}:" +
                $"{slot.Won}:{slot.Forfeited}"));
        var games = String.Join("|", match.Games.Select(game => game.GetRawText()));
        return $"{match.State}:{match.Round}:{match.Number}:{match.WinConditionAmount}:" +
               $"{match.Forfeit}:{slots}:{games}";
    }
}
