# PlayCEA library and new API discovery

Date investigated: 2026-09-07

Scope: document the existing `PlayCEASharp` library and identify the
read-only API surface needed for a new `playcealib`, starting with competition
`1000000113`.

## Existing library summary

`PlayCEASharp` is a C# client and analysis layer for the former
`app.playcea.com` API. Its high-level object graph is:

```text
League
  BracketSet
    Bracket
      Team
      BracketRound
        MatchResult
          Game
```

It scopes data with JSON configuration (`TournamentConfiguration`) and then
hydrates resources, computes statistics/rankings, and maintains lookup tables
for consumers such as the Discord bot.

### Functionality provided

| Area | Functionality |
| --- | --- |
| Discovery and hydration | Load tournaments, filter them by configured game/season criteria, load referenced brackets and teams, and marshal the former compact JSON schema into object models. |
| Competition traversal | Traverse tournaments, bracket sets, brackets, rounds, matches, games, teams, and players; distinguish byes and non-byes; inspect completion, scores, rankings, and team membership. |
| Analysis | Compute per-team total, per-round, cumulative, stage-cumulative, and stage statistics; assign round rankings; derive week numbers and next-match lookups. |
| Caching and refresh | Cache tournament/bracket/team/round resources, refresh periodically in a background thread, and detect newly appearing rounds or changed match scores. |
| Consumer conveniences | Global lookups by player name/Discord ID, team-to-league lookup, match lookup, and events for new rounds and updated matches. |
| Configuration | Match tournaments by configured criteria and customize team/stat naming. |
| Test/administrative tools | Print team IDs/statistics, generate seed/order strings, find score differentials, and print score-report reminders. |

### Read-only flows

These are the flows to preserve in the first `playcealib` release:

- `GET /tournaments`: enumerate tournaments.
- `GET /tournaments/{tournamentId}`: read one tournament.
- `GET /brackets/{bracketId}`: read a complete bracket, including rounds,
  matches, games, teams, and bracket metadata.
- `GET /teams/{teamId}`: read team details and players.
- `GET /matches/{matchId}`: read the latest match result.
- `LeagueManager.Bootstrap`, `ForceUpdate`, and its five-minute refresh loop:
  compose the reads above into configured league objects.
- Local analysis and derived lookups: statistics, rankings, next matches,
  player/team indexes, and update detection. These are local computations,
  not server writes.

The old read path is tightly coupled to a global singleton
(`LeagueManager`) and synchronous `.Result`/`Task.WaitAll` calls. A new
library should not assume that shape; the new API is naturally modeled as
async, ID-based resource reads with explicit pagination and expansion.

### Read/write flows

These former methods mutate PlayCEA and are intentionally out of scope for
the initial read-only library:

| Method | Former request |
| --- | --- |
| `RequestManager.ReportScores` | `POST /matches/{matchId}/scores`, bearer token, game scores |
| `RequestManager.CreateTeam` | `POST /teams`, bearer token, team name/org/charity |
| `RequestManager.AddTeamToTournament` | `PUT /tournament/{tournamentId}/teams`, bearer token |
| `RequestManager.GenerateNewInviteCode` | `POST /teams/{teamId}/invite`, bearer token |

The administrative console also contains score-reporting code, but no write
flow should be carried into `playcealib` until authentication, permissions,
mutation schemas, and error semantics are investigated.

## New site investigation

Page investigated:
`https://app.playcea.com/competition/1000000113/bracket/1000000323`

The page is a Next.js frontend for Rally Cry. Its production API host is:

```text
https://urc-api-590668323850.us-central1.run.app
```

The frontend creates a tRPC client at:

```text
{apiHost}/trpc
```

Public queries use `GET /trpc/{procedure}?input={url-encoded JSON}`. The input
is wrapped in a tRPC `json` property, for example:

```text
GET /trpc/competition.getById?input={"json":{"id":1000000113}}
```

Responses are tRPC envelopes (`result.data.json`) rather than the former
`{ data: [...] }` REST envelope. The public page allowed these reads without an
explicit authentication header during this investigation.

### Competition `1000000113`

Confirmed response:

```text
competition.getById
  id: 1000000113
  name: Rocket League - Fall 2026
  game.id: 24663027358978048
  organization.id: 164
  state: IN_PROGRESS
```

`competition.brackets.list` returned these child brackets:

| ID | Name | Description | State | Kind/notes |
| ---: | --- | --- | --- | --- |
| 1000000322 | Legacy | Stage 1 | STARTED | Native |
| 1000000323 | Contenders | Stage 1 | STARTED | Native; page target |
| 1000000319 | Scrims | — | STARTED | Has `canAssignIfNot: 3` |

### Read surfaces mapped

| tRPC procedure | Input observed | Useful data / replacement |
| --- | --- | --- |
| `competition.getById` | `{ id: competitionId }` | Competition identity, game, organization, state, linked resource IDs. Replaces tournament identity/config discovery. |
| `competition.brackets.list` | `{ competitionId }` | All brackets under a competition. Replaces tournament bracket references and provides the starting point for `1000000322`, `1000000323`, and `1000000319`. |
| `competition.bracket.getById` | `{ id: bracketId }` | Bracket identity, competition, name, state, settings, `maxRound`. |
| `competition.bracket.graph.get` | `{ bracketId }` | Bracket graph: metadata, rounds, nodes, slots, entry IDs/names, scheduled/open state, round/number, game count, win condition, scores/forfeits when present. This is the closest direct replacement for the old bracket payload. |
| `competition.match.list` | `{ bracketId, pageSize }` | Match records, assignments, slots, event/channel references, round/number, state, start time, win condition, and game summaries. The target bracket currently returned five week-one matches. |
| `competition.matches.getById` | `{ id, expand: { slots, assignments, settings, bracket }, withProjectedStarts: true }` | Detailed match, expanded bracket/entry/assignment records, slots, games, and result fields. Closest replacement for `GetMatchResult`. |
| `competition.entries.list` | `{ competitionId, pageSize }` | Competition entries (the new team-like unit), alternate name, representing community, leader, team/image references, lock/join state. Competition `1000000113` currently returned 20 entries. |
| `competition.bracket.assignments.list` | `{ bracketId }` | Entry-to-bracket membership and assignment IDs. Required to connect entries to a bracket independently of match slots. |
| `competition.entry.roster.roster` | `{ entryId }` | Roster members with display name/image, participant/user IDs, leader/authority flags, competition role, allocation, and Discord presence. This replaces old team players/captains. |
| `competition.entry.members.list` | `{ entryId }` | Compact membership records; useful when the richer roster view is unnecessary. |
| `competition.core.events.list` | `{ competitionId }` | Competition schedule/events, including check-in, registration, bracket start, and match-week events. |
| `competition.core.stats` | `{ competitionId }` | Counts for entries, players, brackets, events, requests, and officers. |
| `competition.profile.getByCompetitionId` | `{ competitionId }` | Public competition profile/banner metadata. |

The target `Contenders` graph was a Swiss bracket. Its graph nodes include
entry names directly, while match details retain normalized IDs and expanded
entry records. A new client should keep both the normalized records and the
graph projection rather than assuming every bracket is elimination-shaped.

### Direct replacements

The first read-only implementation can replace the important old flows with:

1. Read the competition with `competition.getById`.
2. Enumerate child brackets with `competition.brackets.list`.
3. Read each bracket’s identity and graph with
   `competition.bracket.getById` and `competition.bracket.graph.get`.
4. Read paged matches with `competition.match.list`, then hydrate selected
   matches with `competition.matches.getById`.
5. Read entries and bracket assignments, then hydrate rosters per entry.
6. Perform statistics/rankings locally over graph/match/game data, retaining
   the old analysis as a separate layer.

This covers competition/bracket/team-like status, match status, scheduled
start time, entry membership, rosters, and match scores/results when scores
exist.

### Additional investigation required

- **Pagination and limits:** the frontend uses both `pageSize` and infinite
  queries. Confirm cursor names, maximum page sizes, and whether all list
  procedures use `totalItems`, `nextCursor`, or another envelope.
- **Expansion contract:** determine the supported `expand` values and whether
  expanded entries/communities/participants can be requested in one call.
  Avoid one roster request per entry if a bulk read exists.
- **Team semantics:** the old model’s `Team` maps ambiguously to a new
  `competition.entry`, `team`, `representing` community, and roster. Decide
  which IDs and names are stable public identifiers in `playcealib`.
- **Scores and games:** current target matches have empty `games`; inspect a
  completed match later to document game-level score shape, victor,
  forfeits, and state transitions.
- **Bye and pending semantics:** verify how Swiss byes, unassigned slots,
  dropped entries, and pending matches are represented. Do not assume the old
  one-team bye convention.
- **Standings/statistics:** `competition.standing.get` and
  `competition.standing.heat.get` returned empty arrays for this current
  bracket. Determine whether standings are generated only after results,
  require a different input, or are exposed through the graph/another
  procedure.
- **Live updates:** the frontend subscribes to entity invalidation and a
  WebSocket host (`wss://urc-ws-590668323850.us-central1.run.app`). Determine
  whether the library should initially poll, consume WebSocket invalidations,
  or support both.
- **Authentication and rate limits:** public reads worked without an
  explicit token, but determine visibility rules for private competitions,
  CORS/user-agent requirements, throttling, and stable error codes.
- **API stability:** these are frontend tRPC procedures, not a published
  compatibility contract. Pin a transport/query abstraction and add fixture
  tests before depending on undocumented response details.
- **Write flows:** score reporting, team/entry creation, joining, invitations,
  and roster mutations need separate permission and mutation investigation
  after read behavior is stable.

## Recommended first `playcealib` boundary

Start with an async, read-only client exposing:

- `get_competition(competition_id)`
- `list_brackets(competition_id)`
- `get_bracket(bracket_id)` and `get_bracket_graph(bracket_id)`
- `list_matches(bracket_id, pagination)`
- `get_match(match_id, expand=...)`
- `list_entries(competition_id, pagination)`
- `list_bracket_assignments(bracket_id)`
- `get_entry_roster(entry_id)`
- `list_competition_events(competition_id)`

Keep transport DTOs separate from domain objects, preserve raw IDs and
timestamps, and make local analysis/refresh optional rather than embedding a
global background thread in the client.
