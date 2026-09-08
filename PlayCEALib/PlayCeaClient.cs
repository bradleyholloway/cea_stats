using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlayCEALib;

/// <summary>
/// Read-only client for the current PlayCEA/Rally Cry tRPC API.
/// </summary>
public sealed partial class PlayCeaClient
{
    public const string ProductionApiHost = "https://urc-api-590668323850.us-central1.run.app";

    private readonly HttpClient httpClient;
    private readonly Uri apiBaseUri;
    private readonly JsonSerializerOptions serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public PlayCeaClient(HttpClient? httpClient = null, Uri? apiBaseUri = null)
    {
        this.httpClient = httpClient ?? new HttpClient();
        this.apiBaseUri = (apiBaseUri ?? new Uri(ProductionApiHost)).EnsureTrailingSlash();
        this.httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("PlayCEALib/0.1");
        this.httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public Task<Competition> GetCompetitionAsync(long competitionId, CancellationToken cancellationToken = default) =>
        QueryAsync<Competition>("competition.getById", new { id = competitionId }, cancellationToken);

    public Task<PlayCeaOrganization> GetOrganizationAsync(
        long organizationId,
        CancellationToken cancellationToken = default) =>
        QueryAsync<PlayCeaOrganization>(
            "organization.getById",
            new { id = organizationId },
            cancellationToken);

    public async Task<IReadOnlyList<CompetitionBracket>> ListBracketsAsync(
        long competitionId, CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<PagedResult<CompetitionBracket>>(
            "competition.brackets.list", new { competitionId }, cancellationToken);
        return result.Items;
    }

    public Task<CompetitionBracket> GetBracketAsync(
        long bracketId, CancellationToken cancellationToken = default) =>
        QueryAsync<CompetitionBracket>("competition.bracket.getById",
            new { id = bracketId }, cancellationToken);

    public Task<BracketGraph> GetBracketGraphAsync(
        long bracketId, CancellationToken cancellationToken = default) =>
        QueryAsync<BracketGraph>("competition.bracket.graph.get",
            new { bracketId }, cancellationToken);

    public async Task<PagedResult<CompetitionMatch>> ListMatchesAsync(
        long bracketId, int pageSize = 200, CancellationToken cancellationToken = default)
    {
        return await ListMatchesPageAsync(bracketId, pageSize, null, cancellationToken);
    }

    public async Task<PagedResult<CompetitionMatch>> ListMatchesPageAsync(
        long bracketId, int pageSize = 200, string? cursor = null,
        CancellationToken cancellationToken = default)
    {
        ValidatePageSize(pageSize);
        return await QueryAsync<PagedResult<CompetitionMatch>>(
            "competition.match.list", new { bracketId, pageSize, cursor }, cancellationToken);
    }

    public Task<IReadOnlyList<CompetitionMatch>> ListAllMatchesAsync(
        long bracketId, int pageSize = 200, CancellationToken cancellationToken = default) =>
        ReadAllPagesAsync(
            cursor => ListMatchesPageAsync(bracketId, pageSize, cursor, cancellationToken));

    public Task<CompetitionMatch> GetMatchAsync(
        long matchId, CancellationToken cancellationToken = default) =>
        QueryAsync<CompetitionMatch>("competition.matches.getById", new
        {
            id = matchId,
            expand = new { slots = true, assignments = true, settings = true, bracket = true },
            withProjectedStarts = true
        }, cancellationToken);

    public async Task<PagedResult<CompetitionEntry>> ListEntriesAsync(
        long competitionId, int pageSize = 200, CancellationToken cancellationToken = default)
    {
        return await ListEntriesPageAsync(competitionId, pageSize, null, cancellationToken);
    }

    public async Task<PagedResult<CompetitionEntry>> ListEntriesPageAsync(
        long competitionId, int pageSize = 200, string? cursor = null,
        CancellationToken cancellationToken = default)
    {
        ValidatePageSize(pageSize);
        return await QueryAsync<PagedResult<CompetitionEntry>>(
            "competition.entries.list", new { competitionId, pageSize, cursor }, cancellationToken);
    }

    public Task<IReadOnlyList<CompetitionEntry>> ListAllEntriesAsync(
        long competitionId, int pageSize = 200, CancellationToken cancellationToken = default) =>
        ReadAllPagesAsync(
            cursor => ListEntriesPageAsync(competitionId, pageSize, cursor, cancellationToken));

    public async IAsyncEnumerable<IReadOnlyList<CompetitionMatch>> PollMatchesAsync(
        long bracketId,
        TimeSpan interval,
        int pageSize = 200,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(interval), interval,
                "Polling interval must be positive.");
        }

        while (true)
        {
            yield return await ListAllMatchesAsync(bracketId, pageSize, cancellationToken);
            await Task.Delay(interval, cancellationToken);
        }
    }

    public async Task<IReadOnlyList<BracketAssignment>> ListBracketAssignmentsAsync(
        long bracketId, CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<PagedResult<BracketAssignment>>(
            "competition.bracket.assignments.list", new { bracketId }, cancellationToken);
        return result.Items;
    }

    public Task<EntryRoster> GetEntryRosterAsync(
        long entryId, CancellationToken cancellationToken = default) =>
        QueryAsync<EntryRoster>("competition.entry.roster.roster",
            new { entryId }, cancellationToken);

    public async Task<IReadOnlyList<EntryMember>> ListEntryMembersAsync(
        long entryId, CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<PagedResult<EntryMember>>(
            "competition.entry.members.list", new { entryId }, cancellationToken);
        return result.Items;
    }

    public async Task<IReadOnlyList<CompetitionEvent>> ListCompetitionEventsAsync(
        long competitionId, CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<PagedResult<CompetitionEvent>>(
            "competition.core.events.list", new { competitionId }, cancellationToken);
        return result.Items;
    }

    public Task<CompetitionStats> GetCompetitionStatsAsync(
        long competitionId, CancellationToken cancellationToken = default) =>
        QueryAsync<CompetitionStats>("competition.core.stats",
            new { competitionId }, cancellationToken);

    public Task<CompetitionProfile> GetCompetitionProfileAsync(
        long competitionId, CancellationToken cancellationToken = default) =>
        QueryAsync<CompetitionProfile>("competition.profile.getByCompetitionId",
            new { competitionId }, cancellationToken);

    public Task<PlayCeaUser> GetUserAsync(
        long idOrKey, CancellationToken cancellationToken = default) =>
        QueryAsync<PlayCeaUser>("user.getByIdOrKey",
            new { idOrKey }, cancellationToken);

    public async Task<IReadOnlyList<UserCommunity>> ListUserCommunitiesAsync(
        long userId, CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<PagedResult<UserCommunity>>(
            "user.lifecycle.communities", new { userId }, cancellationToken);
        return result.Items;
    }

    public async Task<IReadOnlyList<ProfileGame>> ListProfileGamesAsync(
        long userId, long organizationId, CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<PagedResult<ProfileGame>>(
            "profile.games.list",
            new { userId, expand = new { game = true }, organizationId },
            cancellationToken);
        return result.Items;
    }

    public async Task<IReadOnlyList<ProfileContactAccount>> ListProfileContactAccountsAsync(
        long userId, long? communityId = null,
        CancellationToken cancellationToken = default)
    {
        object input = communityId.HasValue
            ? new { userId, communityId = communityId.Value }
            : new { userId };
        var result = await QueryAsync<PagedResult<ProfileContactAccount>>(
            "profile.contactAccounts.sync.list", input, cancellationToken);
        return result.Items;
    }

    public async Task<UserLinkedData> GetUserLinkedDataAsync(
        long userId, long organizationId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserAsync(userId, cancellationToken);
        var communities = await ListUserCommunitiesAsync(userId, cancellationToken);
        var games = await ListProfileGamesAsync(userId, organizationId, cancellationToken);
        var contactAccounts = await ListProfileContactAccountsAsync(userId, null, cancellationToken);
        return new UserLinkedData
        {
            User = user,
            Communities = communities,
            Games = games,
            ContactAccounts = contactAccounts
        };
    }

    public async Task<CompetitionDiscovery> DiscoverCompetitionAsync(
        long competitionId,
        long organizationId,
        CancellationToken cancellationToken = default)
    {
        var competition = await GetCompetitionAsync(competitionId, cancellationToken);
        var brackets = await ListBracketsAsync(competitionId, cancellationToken);
        var entries = await ListAllEntriesAsync(
            competitionId, cancellationToken: cancellationToken);

        var bracketData = await Task.WhenAll(brackets.Select(async bracket =>
        {
            var graphTask = GetBracketGraphAsync(bracket.Id, cancellationToken);
            var assignmentsTask = ListBracketAssignmentsAsync(bracket.Id, cancellationToken);
            var matchesTask = ListAllMatchesAsync(
                bracket.Id, cancellationToken: cancellationToken);
            await Task.WhenAll(graphTask, assignmentsTask, matchesTask);
            return new
            {
                BracketId = bracket.Id,
                Graph = await graphTask,
                Assignments = await assignmentsTask,
                Matches = await matchesTask
            };
        }));

        var assignmentsByBracketId = bracketData.ToDictionary(
            data => data.BracketId,
            data => (IReadOnlyList<BracketAssignment>)data.Assignments);
        var graphsByBracketId = bracketData.ToDictionary(
            data => data.BracketId,
            data => data.Graph);
        var matchesByBracketId = bracketData.ToDictionary(
            data => data.BracketId,
            data => (IReadOnlyList<CompetitionMatch>)data.Matches);

        var involvedEntryIds = bracketData
            .SelectMany(data => data.Assignments)
            .Where(assignment => assignment.Entry != null)
            .Select(assignment => assignment.Entry!.Id)
            .Distinct()
            .ToList();

        var rosterResults = await Task.WhenAll(involvedEntryIds.Select(async entryId =>
            (EntryId: entryId, Roster: await GetEntryRosterAsync(entryId, cancellationToken))));
        var rostersByEntryId = rosterResults.ToDictionary(
            result => result.EntryId,
            result => result.Roster);

        var membershipsByPlayerId = new Dictionary<long, List<RosterMembership>>();
        foreach (var rosterResult in rosterResults)
        {
            foreach (var member in rosterResult.Roster.Members)
            {
                if (!member.Participant.HasValue)
                {
                    continue;
                }

                if (!membershipsByPlayerId.TryGetValue(member.Participant.Value, out var memberships))
                {
                    memberships = new List<RosterMembership>();
                    membershipsByPlayerId.Add(member.Participant.Value, memberships);
                }

                memberships.Add(new RosterMembership
                {
                    EntryId = rosterResult.EntryId,
                    RosterMemberId = member.Id,
                    DisplayName = member.Name ?? member.User?.Name,
                    Leader = member.Leader
                });
            }
        }

        using var linkedDataConcurrency = new SemaphoreSlim(3);
        var players = await Task.WhenAll(membershipsByPlayerId.Keys.Select(
            async platformUserId =>
            {
                await linkedDataConcurrency.WaitAsync(cancellationToken);
                try
                {
                    return (
                        PlatformUserId: platformUserId,
                        LinkedData: await GetUserLinkedDataWithRetryAsync(
                            platformUserId, organizationId, cancellationToken));
                }
                finally
                {
                    linkedDataConcurrency.Release();
                }
            }));
        var playersByPlatformUserId = players.ToDictionary(
            player => player.PlatformUserId,
            player => new DiscoveredPlayer
            {
                PlatformUserId = player.PlatformUserId,
                LinkedData = player.LinkedData,
                Memberships = membershipsByPlayerId[player.PlatformUserId]
            });

        return new CompetitionDiscovery
        {
            Competition = competition,
            Brackets = brackets,
            Entries = entries,
            GraphsByBracketId = graphsByBracketId,
            AssignmentsByBracketId = assignmentsByBracketId,
            MatchesByBracketId = matchesByBracketId,
            RostersByEntryId = rostersByEntryId,
            PlayersByPlatformUserId = playersByPlatformUserId
        };
    }

    private async Task<UserLinkedData> GetUserLinkedDataWithRetryAsync(
        long userId, long organizationId, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await GetUserLinkedDataAsync(userId, organizationId, cancellationToken);
            }
            catch (PlayCeaApiException exception)
                when (exception.ApiStatusCode == HttpStatusCode.TooManyRequests && attempt < 4)
            {
                await Task.Delay(TimeSpan.FromSeconds(attempt), cancellationToken);
            }
        }
    }

    private async Task<T> QueryAsync<T>(
        string procedure, object input, CancellationToken cancellationToken)
    {
        var inputJson = JsonSerializer.Serialize(new { json = input }, serializerOptions);
        var uri = new Uri(apiBaseUri, $"trpc/{procedure}?input={Uri.EscapeDataString(inputJson)}");
        using var response = await GetWithRateLimitRetryAsync(uri, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new PlayCeaApiException(procedure, response.StatusCode, responseBody);
        }

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var result = document.RootElement.GetProperty("result");
            var data = result.GetProperty("data");
            var json = data.GetProperty("json");
            return json.Deserialize<T>(serializerOptions)
                ?? throw new PlayCeaApiException(procedure, response.StatusCode,
                    "The API returned an empty result.");
        }
        catch (JsonException exception)
        {
            throw new PlayCeaApiException(procedure, response.StatusCode,
                "The API returned an invalid tRPC response.", exception);
        }
        catch (KeyNotFoundException exception)
        {
            throw new PlayCeaApiException(procedure, response.StatusCode,
                "The API response did not contain a tRPC result envelope.", exception);
        }
    }

    private async Task<HttpResponseMessage> GetWithRateLimitRetryAsync(
        Uri uri, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            var response = await httpClient.GetAsync(uri, cancellationToken);
            if (response.StatusCode != HttpStatusCode.TooManyRequests || attempt >= 4)
            {
                return response;
            }

            var delay = response.Headers.RetryAfter?.Delta
                ?? TimeSpan.FromSeconds(attempt);
            response.Dispose();
            await Task.Delay(delay, cancellationToken);
        }
    }

    private static void ValidatePageSize(int pageSize)
    {
        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize,
                "Page size must be positive.");
        }
    }

    private static async Task<IReadOnlyList<T>> ReadAllPagesAsync<T>(
            Func<string?, Task<PagedResult<T>>> readPage)
        {
            var items = new List<T>();
            var cursors = new HashSet<string>(StringComparer.Ordinal);
            string? cursor = null;

            do
            {
                var page = await readPage(cursor);
                items.AddRange(page.Items);
                cursor = page.NextCursor;
            }
            while (cursor != null && cursors.Add(cursor));

            if (cursor != null)
            {
                throw new InvalidOperationException(
                    "The API returned a repeated pagination cursor.");
            }

            return items;
    }
}

public sealed class PlayCeaApiException : HttpRequestException
{
    public PlayCeaApiException(
        string procedure, HttpStatusCode statusCode, string responseBody, Exception? innerException = null)
        : base($"PlayCEA procedure '{procedure}' failed with {(int)statusCode} ({statusCode}).",
            innerException)
    {
        Procedure = procedure;
        ApiStatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public string Procedure { get; }
    public HttpStatusCode ApiStatusCode { get; }
    public string ResponseBody { get; }
}

internal static class UriExtensions
{
    public static Uri EnsureTrailingSlash(this Uri uri)
    {
        var value = uri.AbsoluteUri.EndsWith("/", StringComparison.Ordinal)
            ? uri.AbsoluteUri
            : uri.AbsoluteUri + "/";
        return new Uri(value, UriKind.Absolute);
    }
}
