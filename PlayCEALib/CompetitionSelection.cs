using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace PlayCEALib;

public enum OrganizationLookupStatus
{
    Found,
    NotFound,
    Unavailable
}

public sealed class OrganizationLookupResult
{
    public OrganizationLookupStatus Status { get; init; }
    public PlayCeaOrganization? Organization { get; init; }
    public string? Message { get; init; }

    public static OrganizationLookupResult Found(PlayCeaOrganization organization) => new()
    {
        Status = OrganizationLookupStatus.Found,
        Organization = organization
    };

    public static OrganizationLookupResult NotFound(string? message = null) => new()
    {
        Status = OrganizationLookupStatus.NotFound,
        Message = message
    };

    public static OrganizationLookupResult Unavailable(string? message = null) => new()
    {
        Status = OrganizationLookupStatus.Unavailable,
        Message = message
    };
}

public interface IOrganizationDataSource
{
    Task<OrganizationLookupResult> GetOrganizationAsync(
        long organizationId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Resolves organizations through the public PlayCEA organization endpoint.
/// </summary>
public sealed class PlayCeaOrganizationDataSource : IOrganizationDataSource
{
    private readonly PlayCeaClient client;

    public PlayCeaOrganizationDataSource(PlayCeaClient client)
    {
        this.client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<OrganizationLookupResult> GetOrganizationAsync(
        long organizationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var organization = await client.GetOrganizationAsync(
                organizationId,
                cancellationToken);
            return OrganizationLookupResult.Found(organization);
        }
        catch (PlayCeaApiException exception)
            when (exception.ApiStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return OrganizationLookupResult.NotFound(
                $"Organization {organizationId} was not found.");
        }
        catch (PlayCeaApiException exception)
            when (exception.ApiStatusCode == System.Net.HttpStatusCode.Unauthorized ||
                  exception.ApiStatusCode == System.Net.HttpStatusCode.Forbidden ||
                  exception.ApiStatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                  (int)exception.ApiStatusCode >= 500)
        {
            return OrganizationLookupResult.Unavailable(
                $"Organization {organizationId} could not be read: " +
                $"{(int)exception.ApiStatusCode} ({exception.ApiStatusCode}).");
        }
        catch (HttpRequestException exception)
        {
            return OrganizationLookupResult.Unavailable(
                $"Organization {organizationId} could not be read: {exception.Message}");
        }
    }
}

public sealed class CompetitionSelectionRequest
{
    public IReadOnlyCollection<long> CompetitionIds { get; init; } = Array.Empty<long>();
    public string? Organization { get; init; }
    public int PageSize { get; init; } = 50;
    public string? Cursor { get; init; }
    public bool HydrateOrganizations { get; init; } = true;
}

public sealed class CompetitionSelectionItem
{
    public Competition Competition { get; init; } = new();
    public PlayCeaOrganization? Organization { get; init; }
    public OrganizationLookupStatus OrganizationStatus { get; init; }
    public string? OrganizationStatusMessage { get; init; }
}

public sealed class CompetitionSelectionPage
{
    public int TotalItems { get; init; }
    public IReadOnlyList<CompetitionSelectionItem> Items { get; init; } =
        Array.Empty<CompetitionSelectionItem>();
    public string? NextCursor { get; init; }
}

public sealed class OrganizationDataUnavailableException : InvalidOperationException
{
    public OrganizationDataUnavailableException(string message)
        : base(message)
    {
    }
}

public sealed partial class PlayCeaClient
{
    private const int MaximumCompetitionSelectionSize = 500;
    private const int MaximumCompetitionPageSize = 200;

    /// <summary>
    /// Selects a deterministic page from an explicit set of competition IDs.
    /// Organization values containing only an integer are matched by ID;
    /// other values are matched exactly by name, ignoring case.
    /// </summary>
    public async Task<CompetitionSelectionPage> SelectCompetitionsAsync(
        CompetitionSelectionRequest request,
        IOrganizationDataSource? organizationDataSource = null,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.PageSize <= 0 || request.PageSize > MaximumCompetitionPageSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.PageSize),
                request.PageSize,
                $"Page size must be between 1 and {MaximumCompetitionPageSize}.");
        }

        if (request.CompetitionIds == null)
        {
            throw new ArgumentException(
                "Competition IDs cannot be null.",
                nameof(request));
        }

        var competitionIds = request.CompetitionIds
            .Distinct()
            .OrderBy(id => id)
            .ToArray();
        if (competitionIds.Length == 0)
        {
            throw new ArgumentException(
                "At least one competition ID is required.",
                nameof(request));
        }

        if (competitionIds.Length > MaximumCompetitionSelectionSize)
        {
            throw new ArgumentException(
                $"No more than {MaximumCompetitionSelectionSize} competition IDs can be selected.",
                nameof(request));
        }

        var organizationFilter = request.Organization?.Trim();
        var hasOrganizationFilter = !String.IsNullOrEmpty(organizationFilter);
        var organizationId = 0L;
        var filterIsId = hasOrganizationFilter &&
                         Int64.TryParse(
                             organizationFilter,
                             NumberStyles.None,
                             CultureInfo.InvariantCulture,
                             out organizationId);
        var needsOrganizationData = request.HydrateOrganizations ||
                                    (hasOrganizationFilter && !filterIsId);

        var competitions = await Task.WhenAll(
            competitionIds.Select(id => GetCompetitionAsync(id, cancellationToken)));

        var organizationLookups = new Dictionary<long, OrganizationLookupResult>();
        if (needsOrganizationData)
        {
            var organizationIds = competitions
                .Where(competition => competition.Organization != null)
                .Select(competition => competition.Organization!.Id)
                .Distinct()
                .OrderBy(id => id)
                .ToArray();

            if (organizationDataSource == null)
            {
                foreach (var id in organizationIds)
                {
                    organizationLookups[id] = OrganizationLookupResult.Unavailable(
                        "No organization data source was supplied.");
                }
            }
            else
            {
                var lookups = await Task.WhenAll(organizationIds.Select(async id =>
                    (
                        Id: id,
                        Lookup: await organizationDataSource.GetOrganizationAsync(
                            id,
                            cancellationToken)
                    )));
                foreach (var lookup in lookups)
                {
                    if (lookup.Lookup == null)
                    {
                        throw new InvalidOperationException(
                            $"The organization data source returned null for organization {lookup.Id}.");
                    }

                    organizationLookups.Add(lookup.Id, lookup.Lookup);
                }
            }
        }

        if (hasOrganizationFilter && !filterIsId)
        {
            var unavailableOrganizationIds = competitions
                .Where(competition => competition.Organization != null)
                .Select(competition => competition.Organization!.Id)
                .Distinct()
                .Where(id =>
                    !organizationLookups.TryGetValue(id, out var lookup) ||
                    lookup.Status == OrganizationLookupStatus.Unavailable)
                .OrderBy(id => id)
                .ToArray();
            if (unavailableOrganizationIds.Length > 0)
            {
                throw new OrganizationDataUnavailableException(
                    "Organization name filtering could not be evaluated because data is " +
                    $"unavailable for organization IDs: {String.Join(", ", unavailableOrganizationIds)}.");
            }
        }

        var selected = competitions
            .Where(competition =>
                !hasOrganizationFilter ||
                MatchesOrganization(
                    competition,
                    organizationFilter!,
                    filterIsId,
                    organizationId,
                    organizationLookups))
            .OrderBy(competition => String.IsNullOrWhiteSpace(competition.Name) ? 1 : 0)
            .ThenBy(
                competition => String.IsNullOrWhiteSpace(competition.Name)
                    ? String.Empty
                    : competition.Name,
                StringComparer.OrdinalIgnoreCase)
            .ThenBy(competition => competition.Id)
            .ToArray();

        var cursorSignature = CreateSelectionSignature(competitionIds, organizationFilter);
        var offset = ParseSelectionCursor(request.Cursor, cursorSignature);
        if (offset > selected.Length)
        {
            throw new ArgumentException(
                "The cursor offset is beyond the selected competition set.",
                nameof(request));
        }

        var pageCompetitions = selected
            .Skip(offset)
            .Take(request.PageSize)
            .ToArray();
        var items = pageCompetitions.Select(competition =>
        {
            var organizationReference = competition.Organization;
            if (organizationReference == null)
            {
                return new CompetitionSelectionItem
                {
                    Competition = competition,
                    OrganizationStatus = OrganizationLookupStatus.NotFound,
                    OrganizationStatusMessage = "The competition has no organization reference."
                };
            }

            if (organizationLookups.TryGetValue(organizationReference.Id, out var lookup))
            {
                return new CompetitionSelectionItem
                {
                    Competition = competition,
                    Organization = lookup.Organization,
                    OrganizationStatus = lookup.Status,
                    OrganizationStatusMessage = lookup.Message
                };
            }

            return new CompetitionSelectionItem
            {
                Competition = competition,
                OrganizationStatus = OrganizationLookupStatus.Unavailable,
                OrganizationStatusMessage = request.HydrateOrganizations
                    ? "Organization data was unavailable."
                    : "Organization hydration was not requested."
            };
        }).ToArray();

        var nextOffset = offset + pageCompetitions.Length;
        return new CompetitionSelectionPage
        {
            TotalItems = selected.Length,
            Items = items,
            NextCursor = nextOffset < selected.Length
                ? CreateSelectionCursor(nextOffset, cursorSignature)
                : null
        };
    }

    /// <summary>
    /// Discovers multiple competitions in ascending ID order using the
    /// existing single-competition discovery behavior.
    /// </summary>
    public async Task<IReadOnlyList<CompetitionDiscovery>> DiscoverCompetitionsAsync(
        IReadOnlyCollection<long> competitionIds,
        long organizationId,
        CancellationToken cancellationToken = default)
    {
        if (competitionIds == null)
        {
            throw new ArgumentNullException(nameof(competitionIds));
        }

        var ids = competitionIds.Distinct().OrderBy(id => id).ToArray();
        if (ids.Length == 0)
        {
            throw new ArgumentException(
                "At least one competition ID is required.",
                nameof(competitionIds));
        }

        if (ids.Length > MaximumCompetitionSelectionSize)
        {
            throw new ArgumentException(
                $"No more than {MaximumCompetitionSelectionSize} competition IDs can be discovered.",
                nameof(competitionIds));
        }

        var discoveries = new List<CompetitionDiscovery>(ids.Length);
        foreach (var id in ids)
        {
            discoveries.Add(await DiscoverCompetitionAsync(
                id,
                organizationId,
                cancellationToken));
        }

        return discoveries;
    }

    private static bool MatchesOrganization(
        Competition competition,
        string organizationFilter,
        bool filterIsId,
        long organizationId,
        IReadOnlyDictionary<long, OrganizationLookupResult> organizationLookups)
    {
        if (competition.Organization == null)
        {
            return false;
        }

        if (filterIsId)
        {
            return competition.Organization.Id == organizationId;
        }

        return organizationLookups.TryGetValue(competition.Organization.Id, out var lookup) &&
               lookup.Status == OrganizationLookupStatus.Found &&
               lookup.Organization?.Name != null &&
               lookup.Organization.Name.Trim().Equals(
                   organizationFilter,
                   StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateSelectionSignature(
        IEnumerable<long> competitionIds,
        string? organizationFilter)
    {
        var value = String.Join(",", competitionIds) +
                    "|" +
                    (organizationFilter ?? String.Empty).Trim().ToUpperInvariant();
        using var sha256 = SHA256.Create();
        return BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(value)))
            .Replace("-", String.Empty)
            .ToLowerInvariant();
    }

    private static string CreateSelectionCursor(int offset, string signature) =>
        $"v1:{offset.ToString(CultureInfo.InvariantCulture)}:{signature}";

    private static int ParseSelectionCursor(string? cursor, string expectedSignature)
    {
        if (cursor == null)
        {
            return 0;
        }

        var parts = cursor.Split(':');
        if (parts.Length != 3 ||
            parts[0] != "v1" ||
            !Int32.TryParse(
                parts[1],
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var offset) ||
            offset < 0 ||
            !parts[2].Equals(expectedSignature, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The competition selection cursor is invalid or belongs to another query.",
                nameof(cursor));
        }

        return offset;
    }
}
