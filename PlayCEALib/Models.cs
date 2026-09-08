using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlayCEALib;

public sealed class Competition
{
    public long Id { get; init; }
    public string? Name { get; init; }
    public string? DisambiguatingDescription { get; init; }
    public CompetitionGame? Game { get; init; }
    public ResourceReference? Organization { get; init; }
    public string? State { get; init; }
    public string? ListingMode { get; init; }
    public ResourceReference? Profile { get; init; }
    public ResourceReference? Rules { get; init; }
    public ResourceReference? Settings { get; init; }
    public ResourceReference? Section { get; init; }
    public ResourceReference? Channel { get; init; }
}

public sealed class CompetitionGame
{
    public string? Id { get; init; }
    public string? Name { get; init; }
}

public sealed class CompetitionBracket
{
    public long Id { get; init; }
    public ResourceReference? Competition { get; init; }
    public string? Name { get; init; }
    public string? DisambiguatingDescription { get; init; }
    public int? Ordinal { get; init; }
    public string? State { get; init; }
    public ResourceReference? Settings { get; init; }
    public string? ExternalProvider { get; init; }
    public int? MaxRound { get; init; }
    public int? CanAssignIfNot { get; init; }
}

public sealed class BracketGraph
{
    public BracketGraphMetadata? Meta { get; init; }
    public List<BracketGraphRound> Rounds { get; init; } = new();
    public List<BracketGraphNode> Nodes { get; init; } = new();
}

public sealed class BracketGraphMetadata
{
    public long? BracketId { get; init; }
    public string? Provider { get; init; }
    public string? Kind { get; init; }
    public string? State { get; init; }
    public long? CompetitionId { get; init; }
    public long? SettingsId { get; init; }
}

public sealed class BracketGraphRound
{
    public long Id { get; init; }
    public int? Number { get; init; }
    public bool Filtered { get; init; }
}

public sealed class BracketGraphNode
{
    public long Id { get; init; }
    public int? Round { get; init; }
    public int? Number { get; init; }
    public string? State { get; init; }
    public long? ExternalId { get; init; }
    public int? GamesCount { get; init; }
    public int? WinConditionAmount { get; init; }
    public List<BracketGraphSlot> Slots { get; init; } = new();
}

public sealed class BracketGraphSlot
{
    public int? Index { get; init; }
    public long? AssignmentId { get; init; }
    public long? EntryId { get; init; }
    public string? EntryName { get; init; }
    public string? Image { get; init; }
    public int? RoundsScored { get; init; }
    public int? GamesWon { get; init; }
    public bool Forfeited { get; init; }
}

public sealed class CompetitionMatch
{
    public long Id { get; init; }
    public ResourceReference? Bracket { get; init; }
    public int? Round { get; init; }
    public int? Number { get; init; }
    public string? State { get; init; }
    public List<ResourceReference> Assignments { get; init; } = new();
    public ResourceReference? Event { get; init; }
    public ResourceReference? ActiveChannel { get; init; }
    public long? ExternalId { get; init; }
    public string? WinCondition { get; init; }
    public int? WinConditionAmount { get; init; }
    public bool Forfeit { get; init; }
    public bool CustomStartDate { get; init; }
    public DateTimeOffset? StartDate { get; init; }
    public List<MatchSlot> Slots { get; init; } = new();
    public List<JsonElement> Games { get; init; } = new();
    [JsonPropertyName("_expanded")]
    public JsonElement? Expanded { get; init; }
}

public sealed class MatchSlot
{
    public long Id { get; init; }
    public ResourceReference? Match { get; init; }
    public ResourceReference? Assignment { get; init; }
    public int? Position { get; init; }
    public int? RoundsScored { get; init; }
    public int? GamesWon { get; init; }
    public bool Won { get; init; }
    public bool Forfeited { get; init; }
    public DateTimeOffset? DateCreated { get; init; }
    public DateTimeOffset? DateModified { get; init; }
}

public sealed class CompetitionEntry
{
    public long Id { get; init; }
    public ResourceReference? Competition { get; init; }
    public string? AlternateName { get; init; }
    public ResourceReference? Representing { get; init; }
    public bool Locked { get; init; }
    public bool NotificationOptIn { get; init; }
    public string? JoinRestriction { get; init; }
    public ResourceReference? Leader { get; init; }
    public ResourceReference? Team { get; init; }
    public string? Image { get; init; }
    public DateTimeOffset? DateCreated { get; init; }
    public DateTimeOffset? DateModified { get; init; }
}

public sealed class BracketAssignment
{
    public long Id { get; init; }
    public ResourceReference? Bracket { get; init; }
    public ResourceReference? Entry { get; init; }
    public DateTimeOffset? DateCreated { get; init; }
    public DateTimeOffset? DateModified { get; init; }
}

public sealed class EntryRoster
{
    public bool DisplayRanks { get; init; }
    public List<RosterMember> Members { get; init; } = new();
}

public sealed class RosterMember
{
    public long Id { get; init; }
    public long? Participant { get; init; }
    public string? Name { get; init; }
    public string? Image { get; init; }
    public bool Leader { get; init; }
    public bool IsAuthority { get; init; }
    public string? CompRole { get; init; }
    public string? EffectiveRole { get; init; }
    public bool Allocated { get; init; }
    public bool InTeam { get; init; }
    public bool Drift { get; init; }
    public bool IsFreeAgent { get; init; }
    public bool OnDiscord { get; init; }
    public ResourceReference? CompetitionEntry { get; init; }
    public ResourceReference? Competition { get; init; }
    public RosterUser? User { get; init; }
}

public sealed class RosterUser
{
    public long Id { get; init; }
    public string? Name { get; init; }
    public string? Image { get; init; }
}

public sealed class EntryMember
{
    public long Id { get; init; }
    public ResourceReference? Participant { get; init; }
    public ResourceReference? Competition { get; init; }
    public ResourceReference? Entry { get; init; }
    public bool IsAuthority { get; init; }
    public string? CompRole { get; init; }
    public bool OnDiscord { get; init; }
    public DateTimeOffset? DateCreated { get; init; }
    public DateTimeOffset? DateModified { get; init; }
}

public sealed class CompetitionEvent
{
    public long Id { get; init; }
    public ResourceReference? Competition { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Venue { get; init; }
    public string? Kind { get; init; }
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public bool Hidden { get; init; }
    public bool Pinned { get; init; }
}

public sealed class CompetitionStats
{
    public int Root { get; init; }
    public int Entries { get; init; }
    public int EntryCount { get; init; }
    public int PlayersWithEntryCount { get; init; }
    public int Brackets { get; init; }
    public int Events { get; init; }
    public int Requests { get; init; }
    public int Officers { get; init; }
    public int Aliases { get; init; }
}

public sealed class CompetitionProfile
{
    public long Id { get; init; }
    public string? BannerImage { get; init; }
    public DateTimeOffset? DateCreated { get; init; }
    public DateTimeOffset? DateModified { get; init; }
}

public sealed class PlayCeaUser
{
    public long Id { get; init; }
    public string? Name { get; init; }
    public string? Image { get; init; }
    public bool ActiveRecently { get; init; }
    public string? OwnerType { get; init; }

    /// <summary>
    /// Discord account ID when the profile image is a Discord avatar URL.
    /// This is a derived fallback; prefer an explicit contact account when one
    /// is returned by the API.
    /// </summary>
    [JsonIgnore]
    public ulong? DiscordAccountId
    {
        get
        {
            if (!Uri.TryCreate(Image, UriKind.Absolute, out var imageUri))
            {
                return null;
            }

            var segments = imageUri.AbsolutePath.Split(
                new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            return segments.Length >= 2 &&
                   segments[0].Equals("avatars", StringComparison.OrdinalIgnoreCase) &&
                   UInt64.TryParse(segments[1], out var discordId)
                ? discordId
                : null;
        }
    }
}

public sealed class UserCommunity
{
    public long Id { get; init; }
    public string? Name { get; init; }
    public string? Kind { get; init; }
    public ResourceReference? Organization { get; init; }
    public ResourceReference? Parent { get; init; }
    public ResourceReference? Member { get; init; }
    public string? Level { get; init; }
}

public sealed class ProfileGame
{
    public long Id { get; init; }
    public string? Name { get; init; }
    public ResourceReference? Game { get; init; }
    public ResourceReference? Organization { get; init; }
}

public sealed class ProfileContactAccount
{
    public long Id { get; init; }
    public string? Kind { get; init; }
    public string? Name { get; init; }
    public string? Value { get; init; }
    public string? Username { get; init; }
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; init; }
}

public sealed class UserLinkedData
{
    public PlayCeaUser User { get; init; } = new();
    public IReadOnlyList<UserCommunity> Communities { get; init; } = Array.Empty<UserCommunity>();
    public IReadOnlyList<ProfileGame> Games { get; init; } = Array.Empty<ProfileGame>();
    public IReadOnlyList<ProfileContactAccount> ContactAccounts { get; init; } =
        Array.Empty<ProfileContactAccount>();
}

public sealed class CompetitionDiscovery
{
    public Competition Competition { get; init; } = new();
    public IReadOnlyList<CompetitionBracket> Brackets { get; init; } =
        Array.Empty<CompetitionBracket>();
    public IReadOnlyList<CompetitionEntry> Entries { get; init; } =
        Array.Empty<CompetitionEntry>();
    public IReadOnlyDictionary<long, BracketGraph> GraphsByBracketId { get; init; } =
        new Dictionary<long, BracketGraph>();
    public IReadOnlyDictionary<long, IReadOnlyList<BracketAssignment>> AssignmentsByBracketId { get; init; } =
        new Dictionary<long, IReadOnlyList<BracketAssignment>>();
    public IReadOnlyDictionary<long, IReadOnlyList<CompetitionMatch>> MatchesByBracketId { get; init; } =
        new Dictionary<long, IReadOnlyList<CompetitionMatch>>();
    public IReadOnlyDictionary<long, EntryRoster> RostersByEntryId { get; init; } =
        new Dictionary<long, EntryRoster>();
    public IReadOnlyDictionary<long, DiscoveredPlayer> PlayersByPlatformUserId { get; init; } =
        new Dictionary<long, DiscoveredPlayer>();
}

public sealed class DiscoveredPlayer
{
    public long PlatformUserId { get; init; }
    public UserLinkedData LinkedData { get; init; } = new();
    public IReadOnlyList<RosterMembership> Memberships { get; init; } =
        Array.Empty<RosterMembership>();
}

public sealed class RosterMembership
{
    public long EntryId { get; init; }
    public long RosterMemberId { get; init; }
    public string? DisplayName { get; init; }
    public bool Leader { get; init; }
}

public sealed class ResourceReference
{
    public long Id { get; init; }
}

public sealed class PagedResult<T>
{
    public int? TotalItems { get; init; }
    public List<T> Items { get; init; } = new();
    public string? NextCursor { get; init; }
}
