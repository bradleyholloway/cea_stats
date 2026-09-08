# PlayCEALib implementation status

This project is the replacement read-only client for the current
PlayCEA/Rally Cry backend. The original implementation is in
`../PlayCEASharp/PlayCEASharp`.

## Completed

| Status | New library capability | Current API | Original implementation/reference |
| --- | --- | --- | --- |
| Done | Read a competition by ID | `competition.getById` | `RequestManager.GetTournaments`, `GetTournament`; `DataModel.Tournament` |
| Done | List all child brackets | `competition.brackets.list` | `Marshaller.Tournament` bracket references; `DataModel.Tournament.Brackets` |
| Done | Read bracket metadata | `competition.bracket.getById` | `RequestManager.GetBracket`; `DataModel.Bracket` |
| Done | Read a bracket graph | `competition.bracket.graph.get` | `Marshaller.Bracket`, `BracketRound`, and `MatchResult`; `DataModel.Bracket.Rounds` |
| Done | List bracket matches | `competition.match.list` | `RequestManager.GetBracket`; `DataModel.BracketRound.Matches` |
| Done | Read a detailed match | `competition.matches.getById` | `RequestManager.GetMatchResult`; `DataModel.MatchResult` |
| Done | List competition entries | `competition.entries.list` | `DataModel.Tournament.Teams`; new entries replace the old team-like unit |
| Done | List bracket assignments | `competition.bracket.assignments.list` | Bracket team membership assembled by `Marshaller.Bracket` |
| Done | Read an entry roster | `competition.entry.roster.roster` | `Marshaller.Team` and `Marshaller.Player`; `DataModel.Team.Players` |
| Done | Read compact entry members | `competition.entry.members.list` | `DataModel.Team.Players` |
| Done | Read competition events | `competition.core.events.list` | No direct old equivalent; replaces schedule context around rounds |
| Done | Read competition statistics | `competition.core.stats` | No direct old equivalent; old `AnalysisManager` computes match/team statistics locally |
| Done | Read public competition profile | `competition.profile.getByCompetitionId` | No direct old equivalent |
| Done | Read user identity, linked communities, games, and contact accounts | `user.getByIdOrKey`, `user.lifecycle.communities`, `profile.games.list`, `profile.contactAccounts.sync.list` | `DataModel.Player` identity and team membership; new API keeps platform user IDs separate from Discord account IDs |
| Done | Discover a competition aggregate with rosters and linked player data | `DiscoverCompetitionAsync` orchestration over competition/bracket/entry/roster/user procedures | Replaces manual `LeagueInstanceManager` hydration; returns explicit new-model indexes |
| Done | Async tRPC transport and typed error handling | Production `/trpc` host | `RequestManager.GetStringWithRetryAsync` |

## Remaining

- Cursor pagination is implemented for matches and entries through
  `ListMatchesPageAsync`, `ListEntriesPageAsync`, and all-page helpers. The
  cursor key works with the observed API, but the maximum page size and
  behavior across every list procedure still need confirmation.
- Capture completed-match fixtures and model game-level scores, victor,
  forfeits, byes, and pending/unassigned slots. The target competition was
  still showing open matches with empty `games` arrays during discovery.
- Determine the correct standings procedure/input. Both observed standing
  procedures returned empty arrays for the target bracket at that time.
- The first local-analysis slice is implemented in `Analysis.cs`:
  `BracketAnalysis.CalculateStandings` and `MatchChangeTracker` replace the
  core standing/update behavior without depending on legacy types. Round and
  stage statistics plus next-match lookup remain.
- Add optional polling or WebSocket invalidation support. The frontend uses
  `wss://urc-ws-590668323850.us-central1.run.app`. A cancellation-aware
  `PollMatchesAsync` fallback is now available; WebSocket invalidation remains
  unimplemented because its subscription/authentication contract is unknown.
- Add fixture tests for tRPC envelopes, API errors, pagination, and response
  compatibility before treating frontend procedures as stable API contracts.
- Investigate private-competition authentication, rate limits, and visibility.
- Do not implement writes yet. Score reporting, entry/team changes, roster
  mutations, invitations, and authentication require separate investigation.

## Compatibility evaluation

The new DTOs intentionally remain separate from the old
`PlayCEASharp.DataModel` classes. Inheritance cannot preserve the old API
because of numeric-vs-string IDs, internal construction/setters, and the
different entry/roster and pending-slot semantics. See
`COMPATIBILITY.md` for the migration strategy and adapter boundaries.

## Original functionality intentionally excluded

The following `PlayCEASharp.RequestManagement.RequestManager` methods are
write flows and are not present in this read-only project:

- `ReportScores`
- `CreateTeam`
- `AddTeamToTournament`
- `GenerateNewInviteCode`
