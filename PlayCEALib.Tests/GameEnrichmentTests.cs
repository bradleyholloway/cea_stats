using PlayCEALib;

namespace PlayCEALib.Tests;

public sealed class GameEnrichmentTests
{
    [Fact]
    public async Task EnrichmentCachesAvailableProviderData()
    {
        var client = new PlayCeaClient(
            new HttpClient(),
            new Uri("https://example.test/"));
        var provider = new StubEnrichmentProvider(
            new GameSpecificEnrichment
            {
                ProviderId = "rocket-league",
                GameId = "rocket-league",
                Status = GameSpecificEnrichmentStatus.Available
            });
        var cache = new MemoryGameSpecificEnrichmentCache();
        var discovery = CreateDiscovery();

        await client.EnrichCompetitionAsync(discovery, new[] { provider }, cache);
        await client.EnrichCompetitionAsync(discovery, new[] { provider }, cache);

        Assert.Equal(1, provider.CallCount);
    }

    [Fact]
    public async Task EnrichmentDoesNotCacheUnavailableProviderData()
    {
        var client = new PlayCeaClient(
            new HttpClient(),
            new Uri("https://example.test/"));
        var provider = new StubEnrichmentProvider(
            new GameSpecificEnrichment
            {
                ProviderId = "rocket-league",
                GameId = "rocket-league",
                Status = GameSpecificEnrichmentStatus.Unavailable,
                StatusMessage = "No completed games."
            });
        var cache = new MemoryGameSpecificEnrichmentCache();
        var discovery = CreateDiscovery();

        await client.EnrichCompetitionAsync(discovery, new[] { provider }, cache);
        await client.EnrichCompetitionAsync(discovery, new[] { provider }, cache);

        Assert.Equal(2, provider.CallCount);
    }

    [Fact]
    public async Task EnrichmentCanRequireAvailableData()
    {
        var client = new PlayCeaClient(
            new HttpClient(),
            new Uri("https://example.test/"));
        var provider = new StubEnrichmentProvider(
            new GameSpecificEnrichment
            {
                ProviderId = "rocket-league",
                Status = GameSpecificEnrichmentStatus.Unavailable,
                StatusMessage = "Backend unavailable."
            });

        var exception = await Assert.ThrowsAsync<GameSpecificEnrichmentUnavailableException>(() =>
            client.EnrichCompetitionAsync(
                CreateDiscovery(),
                new[] { provider },
                options: new GameEnrichmentOptions { RequireAvailableData = true }));

        Assert.Equal("Backend unavailable.", exception.Message);
    }

    private static CompetitionDiscovery CreateDiscovery() => new()
    {
        Competition = new Competition
        {
            Id = 42,
            Game = new CompetitionGame { Id = "rocket-league", Name = "Rocket League" }
        }
    };

    private sealed class StubEnrichmentProvider : IGameSpecificEnrichmentProvider
    {
        private readonly GameSpecificEnrichment enrichment;

        public StubEnrichmentProvider(GameSpecificEnrichment enrichment)
        {
            this.enrichment = enrichment;
        }

        public string ProviderId => "rocket-league";
        public int CallCount { get; private set; }

        public bool Supports(Competition competition) =>
            competition.Game?.Id == "rocket-league";

        public Task<GameSpecificEnrichment> EnrichAsync(
            GameEnrichmentContext context,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return Task.FromResult(enrichment);
        }
    }
}
