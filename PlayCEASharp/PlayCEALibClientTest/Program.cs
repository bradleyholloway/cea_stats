using PlayCEALib;

namespace PlayCEALibClientTest;

public static class Program
{
    private const long CompetitionId = 1000000113;

    public static async Task<int> Main(string[] args)
    {
        try
        {
            var client = new PlayCeaClient();

            if (args.Contains("--discover", StringComparer.OrdinalIgnoreCase))
            {
                await RunDiscoveryAsync(client);
                return 0;
            }

            var competition = await client.GetCompetitionAsync(CompetitionId);
            Console.WriteLine($"Competition: {competition.Id} - {competition.Name}");
            Console.WriteLine($"State: {competition.State}");
            Console.WriteLine($"Game: {competition.Game?.Id} - {competition.Game?.Name}");
            Console.WriteLine();

            var brackets = await client.ListBracketsAsync(CompetitionId);
            var allEntries = await client.ListAllEntriesAsync(CompetitionId);
            var entriesById = allEntries.ToDictionary(entry => entry.Id);
            var rostersByEntryId = new Dictionary<long, EntryRoster>();

            foreach (var bracket in brackets.OrderBy(bracket => bracket.Ordinal ?? int.MaxValue))
            {
                await PrintOpenMatchesAsync(
                    client, bracket, entriesById, rostersByEntryId);
            }

            Console.ReadLine();
            return 0;
        }
        catch (PlayCeaApiException exception)
        {
            Console.Error.WriteLine(
                $"PlayCEA API request failed: {exception.Procedure} " +
                $"({(int)exception.ApiStatusCode} {exception.ApiStatusCode})");
            Console.Error.WriteLine(exception.ResponseBody);
            return 1;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    private static async Task RunDiscoveryAsync(PlayCeaClient client)
    {
        var discovery = await client.DiscoverCompetitionAsync(CompetitionId, organizationId: 164);
        Console.WriteLine(
            $"Discovered {discovery.Competition.Name}: " +
            $"{discovery.Brackets.Count} brackets, {discovery.RostersByEntryId.Count} rosters, " +
            $"{discovery.PlayersByPlatformUserId.Count} players");

        foreach (var player in discovery.PlayersByPlatformUserId.Values
                     .OrderBy(player => player.LinkedData.User.Name))
        {
            var teams = player.LinkedData.Communities
                .Where(community => community.Kind == "TEAM")
                .Select(community => community.Name)
                .Where(name => name != null);
            Console.WriteLine(
                $"{player.PlatformUserId}: {player.LinkedData.User.Name} " +
                $"discord={player.LinkedData.User.DiscordAccountId?.ToString() ?? "unknown"} " +
                $"({string.Join(", ", teams)})");
        }
    }

    private static async Task PrintOpenMatchesAsync(
        PlayCeaClient client,
        CompetitionBracket bracket,
        IReadOnlyDictionary<long, CompetitionEntry> entriesById,
        IDictionary<long, EntryRoster> rostersByEntryId)
    {
        var assignments = await client.ListBracketAssignmentsAsync(bracket.Id);
        var entryIdByAssignmentId = assignments
            .Where(assignment => assignment.Entry != null)
            .ToDictionary(assignment => assignment.Id, assignment => assignment.Entry!.Id);

        var matches = await client.ListAllMatchesAsync(bracket.Id);
        var openMatches = matches
            .Where(match => !IsCompleted(match.State))
            .OrderByDescending(match => match.StartDate ?? DateTimeOffset.MinValue)
            .ThenByDescending(match => match.Round ?? int.MinValue)
            .ThenByDescending(match => match.Number ?? int.MinValue)
            .ToList();

        Console.WriteLine($"=== {bracket.Name} ({bracket.Id}) ===");
        if (openMatches.Count == 0)
        {
            Console.WriteLine("No incomplete matches.");
            Console.WriteLine();
            return;
        }

        foreach (var match in openMatches)
        {
            var participants = match.Slots
                .OrderBy(slot => slot.Position ?? int.MaxValue)
                .Select(slot => ResolveEntryId(slot.Assignment?.Id, entryIdByAssignmentId))
                .Where(entryId => entryId.HasValue)
                .Select(entryId => entryId!.Value)
                .Distinct()
                .ToList();

            Console.WriteLine(
                $"Match {match.Id}: round {match.Round}, match {match.Number}, " +
                $"state={match.State}, start={match.StartDate?.ToString("u") ?? "unscheduled"}");
            Console.WriteLine(
                $"  {FormatEntryName(participants.Count > 0 ? participants[0] : null, entriesById)} " +
                $"vs {FormatEntryName(participants.Count > 1 ? participants[1] : null, entriesById)}");

            foreach (var entryId in participants)
            {
                if (!rostersByEntryId.TryGetValue(entryId, out var roster))
                {
                    roster = await client.GetEntryRosterAsync(entryId);
                    rostersByEntryId[entryId] = roster;
                }

                Console.WriteLine($"  Roster for {FormatEntryName(entryId, entriesById)}:");
                if (roster.Members.Count == 0)
                {
                    Console.WriteLine("    (no roster members)");
                    continue;
                }

                foreach (var member in roster.Members)
                {
                    var captain = member.Leader ? " [leader]" : string.Empty;
                    Console.WriteLine($"    {member.Name ?? member.User?.Name ?? member.Id.ToString()}{captain}");
                }
            }

            Console.WriteLine();
        }
    }

    private static long? ResolveEntryId(
        long? assignmentId,
        IReadOnlyDictionary<long, long> entryIdByAssignmentId) =>
        assignmentId.HasValue && entryIdByAssignmentId.TryGetValue(assignmentId.Value, out var entryId)
            ? entryId
            : null;

    private static string FormatEntryName(
        long? entryId,
        IReadOnlyDictionary<long, CompetitionEntry> entriesById) =>
        entryId.HasValue && entriesById.TryGetValue(entryId.Value, out var entry)
            ? $"{entry.AlternateName ?? "(unnamed)"} [{entry.Id}]"
            : "(unassigned)";

    private static bool IsCompleted(string? state) =>
        state != null &&
        (state.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase) ||
         state.Equals("COMPLETE", StringComparison.OrdinalIgnoreCase) ||
         state.Equals("FINISHED", StringComparison.OrdinalIgnoreCase) ||
         state.Equals("CLOSED", StringComparison.OrdinalIgnoreCase));
}
