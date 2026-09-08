# Legacy model compatibility

The new API models should not inherit from the old `PlayCEASharp.DataModel`
classes.

The old models are not suitable base classes for the current API:

- Legacy identifiers are `string`, while the current API uses numeric IDs.
- Legacy constructors and most setters are `internal`, preventing a separate
  library from reliably constructing the old object graph.
- `Team` represented a tournament team, but the current API separates an
  `entry`, its representing community/team, and its roster.
- `MatchResult` assumes home/away teams and a fully materialized `Game` list;
  the current bracket graph can contain pending, unassigned, forfeited, or
  Swiss slots that do not fit that shape.
- The old models contain mutable analysis caches (`TeamStatistics`,
  round rankings, and lookup dictionaries) that should not be coupled to
  transport DTOs.

Consequently, changing only the client package cannot make existing compiled
clients switch invisibly: the CLR identity includes the assembly and the
legacy types' namespaces and members. The safe compatibility approach is a
facade that keeps the existing `PlayCEASharp.RequestManagement` and
`PlayCEASharp.DataModel` contracts, calls `PlayCEALib.PlayCeaClient`
internally, and maps only records that satisfy the old home/away/team
assumptions. New consumers should use `PlayCEALib` directly.

The current library therefore provides compatibility-oriented primitives
without pretending that all records are losslessly convertible:

- `MatchChangeTracker` replaces the old `MatchResultCache` behavior for
  current match DTOs.
- `BracketAnalysis.CalculateStandings` provides local read-only standings
  when completed graph nodes contain score data.
- `CompetitionEntry`, `EntryRoster`, and `RosterMember` preserve the separate
  entry/team/community/roster identifiers needed to build an explicit adapter.
- `GetUserLinkedDataAsync` exposes the current user/community/profile surfaces;
  its `PlayCeaUser.Id` is still a platform user ID, not a Discord ID. Discord
  avatar URLs may contain an account ID, but that is an observed presentation
  detail rather than a stable linked-account contract. `PlayCeaUser` exposes
  this as the derived `DiscordAccountId` fallback while preserving explicit
  contact-account records separately.

An adapter should expose conversion warnings or an explicit partial-result
state for byes, pending slots, and matches whose current API data has no
home/away equivalent.
