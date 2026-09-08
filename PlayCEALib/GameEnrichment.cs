using System.Collections.Concurrent;
using System.Text.Json;

namespace PlayCEALib;

public enum GameSpecificEnrichmentStatus
{
    Unavailable,
    Available
}

public sealed class GameSpecificEnrichment
{
    public string ProviderId { get; init; } = String.Empty;
    public string? GameId { get; init; }
    public GameSpecificEnrichmentStatus Status { get; init; }
    public string? StatusMessage { get; init; }
    public IReadOnlyList<EnrichedMatchData> Matches { get; init; } =
        Array.Empty<EnrichedMatchData>();
    public IReadOnlyDictionary<string, JsonElement> Attributes { get; init; } =
        new Dictionary<string, JsonElement>();
}

public sealed class EnrichedMatchData
{
    public long CompetitionMatchId { get; init; }
    public string? ExternalMatchId { get; init; }
    public int? BestOf { get; init; }
    public IReadOnlyList<EnrichedGameData> Games { get; init; } =
        Array.Empty<EnrichedGameData>();
    public IReadOnlyDictionary<string, JsonElement> Attributes { get; init; } =
        new Dictionary<string, JsonElement>();
}

public sealed class EnrichedGameData
{
    public int Sequence { get; init; }
    public string? ExternalGameId { get; init; }
    public string? State { get; init; }
    public DateTimeOffset? StartedAt { get; init; }
    public DateTimeOffset? EndedAt { get; init; }
    public long? WinnerEntryId { get; init; }
    public IReadOnlyList<EnrichedParticipantResult> Participants { get; init; } =
        Array.Empty<EnrichedParticipantResult>();
    public IReadOnlyDictionary<string, JsonElement> Attributes { get; init; } =
        new Dictionary<string, JsonElement>();
}

public sealed class EnrichedParticipantResult
{
    public long? EntryId { get; init; }
    public string? ExternalParticipantId { get; init; }
    public int? Score { get; init; }
    public bool? Won { get; init; }
    public IReadOnlyDictionary<string, JsonElement> Attributes { get; init; } =
        new Dictionary<string, JsonElement>();
}

public sealed class GameEnrichmentContext
{
    public CompetitionDiscovery Discovery { get; init; } = new();
}

public interface IGameSpecificEnrichmentProvider
{
    string ProviderId { get; }
    bool Supports(Competition competition);
    Task<GameSpecificEnrichment> EnrichAsync(
        GameEnrichmentContext context,
        CancellationToken cancellationToken = default);
}

public interface IGameSpecificEnrichmentCache
{
    Task<GameSpecificEnrichment?> GetAsync(
        string providerId,
        long competitionId,
        CancellationToken cancellationToken = default);

    Task SetAsync(
        string providerId,
        long competitionId,
        GameSpecificEnrichment enrichment,
        TimeSpan duration,
        CancellationToken cancellationToken = default);
}

public sealed class MemoryGameSpecificEnrichmentCache : IGameSpecificEnrichmentCache
{
    private readonly ConcurrentDictionary<(string ProviderId, long CompetitionId), CacheEntry> entries =
        new();

    public Task<GameSpecificEnrichment?> GetAsync(
        string providerId,
        long competitionId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = (providerId, competitionId);
        if (!entries.TryGetValue(key, out var entry))
        {
            return Task.FromResult<GameSpecificEnrichment?>(null);
        }

        if (entry.ExpiresAt > DateTimeOffset.UtcNow)
        {
            return Task.FromResult<GameSpecificEnrichment?>(entry.Enrichment);
        }

        entries.TryRemove(key, out _);
        return Task.FromResult<GameSpecificEnrichment?>(null);
    }

    public Task SetAsync(
        string providerId,
        long competitionId,
        GameSpecificEnrichment enrichment,
        TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(duration),
                duration,
                "Cache duration must be positive.");
        }

        entries[(providerId, competitionId)] = new CacheEntry(
            enrichment,
            DateTimeOffset.UtcNow.Add(duration));
        return Task.CompletedTask;
    }

    private sealed class CacheEntry
    {
        public CacheEntry(GameSpecificEnrichment enrichment, DateTimeOffset expiresAt)
        {
            Enrichment = enrichment;
            ExpiresAt = expiresAt;
        }

        public GameSpecificEnrichment Enrichment { get; }
        public DateTimeOffset ExpiresAt { get; }
    }
}

public sealed class GameEnrichmentOptions
{
    public TimeSpan CacheDuration { get; init; } = TimeSpan.FromMinutes(5);
    public bool RequireAvailableData { get; init; }
}

public sealed class EnrichedCompetitionDiscovery
{
    public CompetitionDiscovery Discovery { get; init; } = new();
    public IReadOnlyList<GameSpecificEnrichment> Enrichments { get; init; } =
        Array.Empty<GameSpecificEnrichment>();
}

public sealed class GameSpecificEnrichmentUnavailableException : InvalidOperationException
{
    public GameSpecificEnrichmentUnavailableException(string message)
        : base(message)
    {
    }
}

public sealed partial class PlayCeaClient
{
    public async Task<EnrichedCompetitionDiscovery> DiscoverCompetitionWithEnrichmentAsync(
        long competitionId,
        long organizationId,
        IReadOnlyCollection<IGameSpecificEnrichmentProvider> providers,
        IGameSpecificEnrichmentCache? cache = null,
        GameEnrichmentOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var discovery = await DiscoverCompetitionAsync(
            competitionId,
            organizationId,
            cancellationToken);
        return await EnrichCompetitionAsync(
            discovery,
            providers,
            cache,
            options,
            cancellationToken);
    }

    public async Task<EnrichedCompetitionDiscovery> EnrichCompetitionAsync(
        CompetitionDiscovery discovery,
        IReadOnlyCollection<IGameSpecificEnrichmentProvider> providers,
        IGameSpecificEnrichmentCache? cache = null,
        GameEnrichmentOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (discovery == null)
        {
            throw new ArgumentNullException(nameof(discovery));
        }

        if (providers == null)
        {
            throw new ArgumentNullException(nameof(providers));
        }

        options ??= new GameEnrichmentOptions();
        if (options.CacheDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                options.CacheDuration,
                "Cache duration must be positive.");
        }

        var providerIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var provider in providers)
        {
            if (provider == null)
            {
                throw new ArgumentException(
                    "The provider collection cannot contain null values.",
                    nameof(providers));
            }

            if (String.IsNullOrWhiteSpace(provider.ProviderId))
            {
                throw new InvalidOperationException(
                    "Enrichment providers must expose a non-empty provider ID.");
            }

            if (!providerIds.Add(provider.ProviderId))
            {
                throw new InvalidOperationException(
                    $"More than one enrichment provider uses ID '{provider.ProviderId}'.");
            }
        }

        var supportingProviders = providers
            .Where(provider => provider.Supports(discovery.Competition))
            .OrderBy(provider => provider.ProviderId, StringComparer.Ordinal)
            .ToArray();
        if (supportingProviders.Length == 0 && options.RequireAvailableData)
        {
            throw new GameSpecificEnrichmentUnavailableException(
                $"No enrichment provider supports competition {discovery.Competition.Id}.");
        }

        var enrichments = new List<GameSpecificEnrichment>(supportingProviders.Length);
        foreach (var provider in supportingProviders)
        {
            var enrichment = cache == null
                ? null
                : await cache.GetAsync(
                    provider.ProviderId,
                    discovery.Competition.Id,
                    cancellationToken);
            if (enrichment == null)
            {
                enrichment = await provider.EnrichAsync(
                    new GameEnrichmentContext { Discovery = discovery },
                    cancellationToken);
                if (enrichment == null)
                {
                    throw new InvalidOperationException(
                        $"Enrichment provider '{provider.ProviderId}' returned null.");
                }

                if (!enrichment.ProviderId.Equals(
                    provider.ProviderId,
                    StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"Enrichment provider '{provider.ProviderId}' returned data for " +
                        $"provider '{enrichment.ProviderId}'.");
                }

                if (cache != null &&
                    enrichment.Status == GameSpecificEnrichmentStatus.Available)
                {
                    await cache.SetAsync(
                        provider.ProviderId,
                        discovery.Competition.Id,
                        enrichment,
                        options.CacheDuration,
                        cancellationToken);
                }
            }

            if (options.RequireAvailableData &&
                enrichment.Status != GameSpecificEnrichmentStatus.Available)
            {
                throw new GameSpecificEnrichmentUnavailableException(
                    enrichment.StatusMessage ??
                    $"Provider '{provider.ProviderId}' could not enrich competition " +
                    $"{discovery.Competition.Id}.");
            }

            enrichments.Add(enrichment);
        }

        return new EnrichedCompetitionDiscovery
        {
            Discovery = discovery,
            Enrichments = enrichments
        };
    }
}
