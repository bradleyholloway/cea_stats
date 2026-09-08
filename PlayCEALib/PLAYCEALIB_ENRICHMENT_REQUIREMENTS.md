# PlayCEALib enrichment requirements

## Purpose and scope

PlayCEALib must give `cea-discord-bot` a read-only, provider-neutral way to
select more than one competition, identify competitions by organization, and
optionally attach game-specific match details. The implementation must not
depend on PlayCEASharp or expose its models.

The requirements distinguish capabilities that can be implemented against the
currently observed API from contracts that need a backend procedure or an
external provider. Missing backend data must be represented explicitly; it
must not be inferred from names, URLs, or unrelated identifiers.

## Current limitations

- The legacy `DiscoverCompetitionAsync(long competitionId,
  long organizationId)` accepts one competition. The additive multi-discovery
  overload still requires one organization ID shared by all requested
  competitions and intentionally processes them sequentially.
- The observed competition DTO contains only an organization resource ID.
  `organization.getById` currently hydrates a name, but there is no verified
  public procedure that lists all competitions for an organization.
- Matches expose a provider-neutral `games` JSON array, but available fixtures
  have not established a stable game schema. Completed Rocket League fixtures,
  player statistics, replay identifiers, and provider authentication are not
  available.
- Cursor behavior is observed for entries and matches only. No verified
  server-side competition-search cursor exists.
- Rate limits, private-competition visibility, and consistency guarantees
  while paging remain undocumented.

## 1. Multi-competition selection

### Public API and DTO requirements

The library shall expose:

- `SelectCompetitionsAsync(CompetitionSelectionRequest,
  IOrganizationDataSource?, CancellationToken)`, returning a
  `CompetitionSelectionPage`.
- `DiscoverCompetitionsAsync(IReadOnlyCollection<long>, long organizationId,
  CancellationToken)`, returning one `CompetitionDiscovery` per distinct ID.
- `CompetitionSelectionRequest` with explicit competition IDs, an optional
  organization filter, page size, cursor, and organization-hydration flag.
- `CompetitionSelectionPage` with `TotalItems`, `Items`, and `NextCursor`.
- `CompetitionSelectionItem` with the competition, optional hydrated
  organization, and an explicit organization lookup status/message.
- `PlayCeaOrganizationDataSource`, the built-in organization source backed by
  `organization.getById`. Applications may substitute another authoritative
  source through `IOrganizationDataSource`.

Until the backend supplies a verified list/search procedure, selection is over
an explicit caller-provided set of competition IDs. A future server-side
implementation may add a separate query DTO, but must preserve the result
semantics below.

### Filtering semantics

1. Duplicate competition IDs are removed.
2. An absent/blank organization filter includes every selected competition.
3. A filter consisting of a base-10 integer is an organization ID and is
   matched against `Competition.Organization.Id`.
4. Any other filter is an organization name. It is trimmed and matched exactly
   using ordinal, case-insensitive comparison. Substring and culture-sensitive
   matching are not permitted.
5. Competitions without an organization reference do not match an organization
   filter.
6. Fetch failures for requested competitions propagate as `PlayCeaApiException`;
   selection must not silently return a partial set.

### Pagination and deterministic ordering

- Page size must be 1 through 200. A single request may contain at most 500
  distinct competition IDs while selection remains client-side.
- Filtering occurs before pagination.
- Results are ordered by non-empty competition name using ordinal,
  case-insensitive ordering, then by numeric competition ID. Missing/blank names
  sort after named competitions.
- Cursors are opaque, versioned, and tied to the normalized competition-ID set
  and organization filter. Reusing a cursor with another query fails.
- `TotalItems` is the post-filter count. `NextCursor` is null on the final page.
- Repeated calls over unchanged backend records produce the same ordering and
  page boundaries.

### Compatibility

`DiscoverCompetitionAsync` remains source- and behavior-compatible and is the
primitive used by `DiscoverCompetitionsAsync`. Multi-discovery is deterministic
by ascending competition ID and is sequential to avoid multiplying the
existing nested roster/user request fan-out.

## 2. Organization name filtering

### Organization contract

The library shall expose a `PlayCeaOrganization` DTO with numeric `Id` and
optional `Name`, plus `IOrganizationDataSource.GetOrganizationAsync`. The
lookup result must distinguish:

- `Found`: an organization DTO was hydrated.
- `NotFound`: the source authoritatively reports no organization.
- `Unavailable`: no source was configured or the source cannot provide data.

The included PlayCEA-backed source calls the current organization procedure.
An application may instead use a trusted database or another authoritative
service. A data source must not use PlayCEASharp.

### Required behavior

- Numeric organization filtering works without name hydration.
- Name filtering requires authoritative organization data. If any referenced
  organization needed to evaluate the page is unavailable, the operation fails
  with `OrganizationDataUnavailableException`; it must not return a
  false-negative partial result.
- `NotFound` is authoritative and does not match a name.
- When no name filter is used, unavailable hydration does not fail selection.
  Each item reports `Unavailable` and keeps the organization resource ID on its
  competition DTO.
- Organization lookups occur once per distinct organization ID per selection
  call and before pagination so filtering and totals are correct.

### Backend/API status

`organization.getById` with `{ id }` was verified against organization `164`
on 2026-09-07 and is used by `PlayCeaOrganizationDataSource`. It remains an
undocumented frontend tRPC procedure rather than a published compatibility
contract. A 404 maps to `NotFound`; authentication, throttling, transport, and
server failures map to `Unavailable`. `IOrganizationDataSource` remains the
stable integration seam if this route changes.

## 3. Game-specific enrichment

### Provider-neutral contract

The library shall expose:

- `IGameSpecificEnrichmentProvider`, identified by a stable `ProviderId`, with
  `Supports(Competition)` and asynchronous enrichment.
- `GameEnrichmentContext`, containing the complete provider-neutral
  `CompetitionDiscovery`.
- `GameSpecificEnrichment`, whose status is `Available` or `Unavailable`, plus
  provider/game IDs, a status message, match data, and extension attributes.
- `EnrichedMatchData`, `EnrichedGameData`, and
  `EnrichedParticipantResult`.
- `EnrichCompetitionAsync` for already-discovered data and
  `DiscoverCompetitionWithEnrichmentAsync` as a convenience composition.

Providers are invoked in ordinal `ProviderId` order. A provider returning null
or a mismatched provider ID is a contract error. Unexpected provider exceptions
propagate. Expected absence is returned as `Unavailable`; callers may set
`RequireAvailableData` to convert absence into
`GameSpecificEnrichmentUnavailableException`.

### Match/game model

- An enriched match links to the PlayCEA numeric match ID and may carry an
  external match ID and best-of count.
- A game has a one-based sequence, optional external ID/state/timestamps,
  optional winning entry ID, and participant results.
- A participant result may link to a PlayCEA entry ID and/or an external
  participant ID and carry score/win state.
- Match, game, participant, and top-level attribute dictionaries hold
  provider-specific JSON fields without contaminating the shared DTO shape.
- Providers must preserve unknown values as absent rather than manufacturing
  zero scores, winners, or completed states.

### Rocket League example

A Rocket League provider could map a PlayCEA match to a Ballchasing group or
Rally Cry provider match, return each replay as an `EnrichedGameData`, map blue
and orange teams to competition entry IDs, and store fields such as overtime
seconds, shots, saves, assists, goals, possession, arena, and playlist in
attribute dictionaries. Player-level telemetry may be represented as
participant attributes only when an authoritative entry/player mapping exists.

The same contract must support other games: a Valorant provider could use maps
as games and rounds won as scores; a fighting-game provider could use sets and
individual games. Consumers must branch on `ProviderId`/`GameId`, not concrete
provider classes in PlayCEALib.

### Optional behavior, failures, and caching

- Enrichment is opt-in. Existing discovery never invokes enrichment.
- No supporting provider yields an empty enrichment list unless enrichment is
  required.
- Expected provider/API absence returns `Unavailable` with a diagnostic
  message. Authentication, malformed data, cancellation, and unexpected
  transport errors propagate rather than being hidden.
- `IGameSpecificEnrichmentCache` is optional. The included memory cache keys by
  provider ID and competition ID and honors a caller-controlled positive TTL.
- Only `Available` results are cached. Unavailable/failure results are retried
  on the next call.
- Providers remain responsible for any finer invalidation key needed for
  match/replay revisions. A future distributed cache can implement the same
  interface.

### Backend/API gaps

Actual Rocket League enrichment remains blocked until at least one supported
source exposes stable match/game identifiers, schemas, credentials, rate
limits, and terms of use, and until completed fixtures establish how PlayCEA
match/entry IDs map to that source. The current work implements the consuming
contract and cache, not a fabricated provider.

## cea-discord-bot consumption

1. Configuration supplies the bot's allowed competition IDs and optional
   organization text.
2. The bot calls `SelectCompetitionsAsync` and follows `NextCursor`; it can use
   numeric organization IDs immediately. For names, it supplies a shared
   `PlayCeaOrganizationDataSource` or another authoritative
   `IOrganizationDataSource`.
3. For each selected item, the bot calls the existing
   `DiscoverCompetitionAsync`, or calls `DiscoverCompetitionsAsync` for the
   selected IDs. Existing single-competition commands require no changes.
4. Commands needing detailed game data call
   `DiscoverCompetitionWithEnrichmentAsync` with explicitly registered
   providers and a shared cache. General standings/roster commands continue to
   consume the provider-neutral discovery.
5. The bot treats `Unavailable` as "details unavailable" and presents the
   status message when useful. It enables `RequireAvailableData` only for
   commands that cannot produce a meaningful response without enrichment.

## Acceptance criteria

- Two or more explicit competition IDs can be selected, deduplicated, sorted,
  and paged reproducibly.
- Invalid page sizes, malformed/stale cursors, empty ID sets, and sets over 500
  fail with argument errors.
- Numeric organization IDs filter without a data source.
- Organization names match exactly regardless of case and surrounding
  whitespace when hydrated data is available.
- `PlayCeaOrganizationDataSource` reads `organization.getById` and maps 404 to
  `NotFound` without relying on PlayCEASharp.
- Name filtering fails explicitly when organization data is unavailable;
  unfiltered selection remains usable and reports hydration status.
- Existing `DiscoverCompetitionAsync` callers compile and execute unchanged.
- Multi-discovery returns distinct competitions in ascending ID order.
- Enrichment is opt-in, provider-neutral, deterministic, cancellation-aware,
  and does not modify base discovery DTOs.
- Available enrichment can be cached; unavailable enrichment is not cached;
  strict callers can require available data.
- The library builds for both `netstandard2.1` and `net8.0` without referencing
  PlayCEASharp.
- A production organization source and Rocket League provider are not claimed
  complete until their backend/API contracts and fixtures are available.
