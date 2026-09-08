using System.Net;
using System.Text;
using System.Text.Json;
using PlayCEALib;

namespace PlayCEALib.Tests;

public sealed class CompetitionSelectionTests
{
    [Fact]
    public async Task SelectCompetitionsPagesInDeterministicOrder()
    {
        var client = CreateClient(new Dictionary<long, (string Name, long OrganizationId)>
        {
            [3] = ("Beta", 20),
            [2] = ("alpha", 10),
            [1] = ("Alpha", 10)
        });

        var firstPage = await client.SelectCompetitionsAsync(
            new CompetitionSelectionRequest
            {
                CompetitionIds = new long[] { 3, 2, 1, 2 },
                PageSize = 2,
                HydrateOrganizations = false
            });
        var secondPage = await client.SelectCompetitionsAsync(
            new CompetitionSelectionRequest
            {
                CompetitionIds = new long[] { 1, 2, 3 },
                PageSize = 2,
                Cursor = firstPage.NextCursor,
                HydrateOrganizations = false
            });

        Assert.Equal(3, firstPage.TotalItems);
        Assert.Equal(new long[] { 1, 2 }, firstPage.Items.Select(item => item.Competition.Id));
        Assert.Equal(new long[] { 3 }, secondPage.Items.Select(item => item.Competition.Id));
        Assert.Null(secondPage.NextCursor);
    }

    [Fact]
    public async Task SelectCompetitionsFiltersNumericOrganizationWithoutHydration()
    {
        var client = CreateClient(new Dictionary<long, (string Name, long OrganizationId)>
        {
            [1] = ("One", 10),
            [2] = ("Two", 20)
        });

        var page = await client.SelectCompetitionsAsync(
            new CompetitionSelectionRequest
            {
                CompetitionIds = new long[] { 1, 2 },
                Organization = "10"
            });

        var item = Assert.Single(page.Items);
        Assert.Equal(1, item.Competition.Id);
        Assert.Equal(OrganizationLookupStatus.Unavailable, item.OrganizationStatus);
    }

    [Fact]
    public async Task SelectCompetitionsFiltersOrganizationNameIgnoringCase()
    {
        var client = CreateClient(new Dictionary<long, (string Name, long OrganizationId)>
        {
            [1] = ("One", 10),
            [2] = ("Two", 20)
        });
        var organizations = new StubOrganizationDataSource(new Dictionary<long, string>
        {
            [10] = "Corporate Esports Association",
            [20] = "Other"
        });

        var page = await client.SelectCompetitionsAsync(
            new CompetitionSelectionRequest
            {
                CompetitionIds = new long[] { 1, 2 },
                Organization = " corporate esports association "
            },
            organizations);

        var item = Assert.Single(page.Items);
        Assert.Equal(1, item.Competition.Id);
        Assert.Equal("Corporate Esports Association", item.Organization?.Name);
        Assert.Equal(OrganizationLookupStatus.Found, item.OrganizationStatus);
    }

    [Fact]
    public async Task SelectCompetitionsRejectsNameFilterWithoutOrganizationData()
    {
        var client = CreateClient(new Dictionary<long, (string Name, long OrganizationId)>
        {
            [1] = ("One", 10)
        });

        await Assert.ThrowsAsync<OrganizationDataUnavailableException>(() =>
            client.SelectCompetitionsAsync(
                new CompetitionSelectionRequest
                {
                    CompetitionIds = new long[] { 1 },
                    Organization = "CEA"
                }));
    }

    [Fact]
    public async Task SelectCompetitionsRejectsCursorFromAnotherQuery()
    {
        var client = CreateClient(new Dictionary<long, (string Name, long OrganizationId)>
        {
            [1] = ("One", 10),
            [2] = ("Two", 10),
            [3] = ("Three", 10)
        });
        var firstPage = await client.SelectCompetitionsAsync(
            new CompetitionSelectionRequest
            {
                CompetitionIds = new long[] { 1, 2, 3 },
                PageSize = 1,
                HydrateOrganizations = false
            });

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.SelectCompetitionsAsync(
                new CompetitionSelectionRequest
                {
                    CompetitionIds = new long[] { 1, 2 },
                    PageSize = 1,
                    Cursor = firstPage.NextCursor,
                    HydrateOrganizations = false
                }));
    }

    [Fact]
    public async Task SelectCompetitionsOrdersBlankNamesById()
    {
        var client = CreateClient(new Dictionary<long, (string Name, long OrganizationId)>
        {
            [3] = (" ", 10),
            [2] = ("Named", 10),
            [1] = ("  ", 10)
        });

        var page = await client.SelectCompetitionsAsync(
            new CompetitionSelectionRequest
            {
                CompetitionIds = new long[] { 3, 2, 1 },
                HydrateOrganizations = false
            });

        Assert.Equal(
            new long[] { 2, 1, 3 },
            page.Items.Select(item => item.Competition.Id));
    }

    [Fact]
    public async Task PlayCeaOrganizationDataSourceHydratesOrganizationName()
    {
        var handler = new OrganizationHttpMessageHandler(
            HttpStatusCode.OK,
            new { id = 164, name = "Corporate Esports Association", key = "cea" });
        var client = new PlayCeaClient(
            new HttpClient(handler),
            new Uri("https://example.test/"));
        var source = new PlayCeaOrganizationDataSource(client);

        var result = await source.GetOrganizationAsync(164);

        Assert.Equal(OrganizationLookupStatus.Found, result.Status);
        Assert.Equal("Corporate Esports Association", result.Organization?.Name);
        Assert.Equal("cea", result.Organization?.Key);
        Assert.Contains("organization.getById", handler.RequestUri?.AbsolutePath);
    }

    [Fact]
    public async Task PlayCeaOrganizationDataSourceMapsNotFound()
    {
        var client = new PlayCeaClient(
            new HttpClient(new OrganizationHttpMessageHandler(
                HttpStatusCode.NotFound,
                new { })),
            new Uri("https://example.test/"));
        var source = new PlayCeaOrganizationDataSource(client);

        var result = await source.GetOrganizationAsync(999);

        Assert.Equal(OrganizationLookupStatus.NotFound, result.Status);
        Assert.Null(result.Organization);
    }

    private static PlayCeaClient CreateClient(
        IReadOnlyDictionary<long, (string Name, long OrganizationId)> competitions)
    {
        var handler = new CompetitionHttpMessageHandler(competitions);
        return new PlayCeaClient(
            new HttpClient(handler),
            new Uri("https://example.test/"));
    }

    private sealed class StubOrganizationDataSource : IOrganizationDataSource
    {
        private readonly IReadOnlyDictionary<long, string> organizations;

        public StubOrganizationDataSource(IReadOnlyDictionary<long, string> organizations)
        {
            this.organizations = organizations;
        }

        public Task<OrganizationLookupResult> GetOrganizationAsync(
            long organizationId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(
                organizations.TryGetValue(organizationId, out var name)
                    ? OrganizationLookupResult.Found(new PlayCeaOrganization
                    {
                        Id = organizationId,
                        Name = name
                    })
                    : OrganizationLookupResult.NotFound());
        }
    }

    private sealed class CompetitionHttpMessageHandler : HttpMessageHandler
    {
        private readonly IReadOnlyDictionary<long, (string Name, long OrganizationId)> competitions;

        public CompetitionHttpMessageHandler(
            IReadOnlyDictionary<long, (string Name, long OrganizationId)> competitions)
        {
            this.competitions = competitions;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var input = GetInput(request.RequestUri!);
            var id = input.GetProperty("json").GetProperty("id").GetInt64();
            var competition = competitions[id];
            var body = JsonSerializer.Serialize(new
            {
                result = new
                {
                    data = new
                    {
                        json = new
                        {
                            id,
                            name = competition.Name,
                            organization = new { id = competition.OrganizationId }
                        }
                    }
                }
            });
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
        }

        private static JsonElement GetInput(Uri uri)
        {
            var query = uri.Query.TrimStart('?').Split('&');
            var inputPair = query.Single(value => value.StartsWith("input=", StringComparison.Ordinal));
            var json = Uri.UnescapeDataString(inputPair.Substring("input=".Length));
            return JsonDocument.Parse(json).RootElement.Clone();
        }
    }

    private sealed class OrganizationHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode statusCode;
        private readonly object organization;

        public OrganizationHttpMessageHandler(
            HttpStatusCode statusCode,
            object organization)
        {
            this.statusCode = statusCode;
            this.organization = organization;
        }

        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            var body = statusCode == HttpStatusCode.OK
                ? JsonSerializer.Serialize(new
                {
                    result = new { data = new { json = organization } }
                })
                : "{}";
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
        }
    }
}
