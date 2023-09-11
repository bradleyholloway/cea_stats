using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PlayCEASharp.DataModel;

namespace PlayCEASharp.Configuration
{
    /// <summary>
    /// Generates the BracketConfiguration based on the MatchingConfiguration.
    /// </summary>
    internal class ConfigurationGenerator
    {
        /// <summary>
        /// Generates the BracketConfiguration given the collection of all tournaments.
        /// </summary>
        /// <param name="allTournaments">All tournaments read from the tournaments endpoint.</param>
        /// <param name="config">The MatchingConfiguration to use to generate the BracketConfiguration.</param>
        /// <returns></returns>
        internal static BracketConfiguration GenerateConfiguration(List<Tournament> allTournaments, MatchingConfiguration config)
        {
            List<Tournament> tournaments = MatchingTournaments(allTournaments, config);

            Dictionary<string, string> stageConfiguration = BuildStageConfigurations(tournaments, config);
            string[][] bracketSets = BuildBracketSets(tournaments, config);
            StageGroup[] stageGroups = BuildStageGroups(tournaments, bracketSets, stageConfiguration, config);
            return new BracketConfiguration()
            {
                bracketSets = bracketSets,
                stageConfiguration = stageConfiguration,
                stageGroups = stageGroups,
            };
        }

        /// <summary>
        /// Filters tournaments that do not match the MatchingConfiguration.
        /// </summary>
        /// <param name="allTournaments">All tournaments from the tournaments endpoint.</param>
        /// <param name="config">The MatchingConfiguration to use to select matching tournaments.</param>
        /// <returns>New list of only tournaments which match the configuration.</returns>
        internal static List<Tournament> MatchingTournaments(List<Tournament> allTournaments, MatchingConfiguration config)
        {
            return allTournaments.Where(t => t.GameId.Equals(config.gameId) && t.SeasonYear.Equals(config.year) && t.SeasonLeague.Equals(config.league) && t.SeasonSeason.Equals(config.season)).ToList();
        }

        /// <summary>
        /// Builds the collection of stage groups.
        /// </summary>
        /// <param name="tournaments">The filtered list of relevant tournaments.</param>
        /// <param name="bracketSets">The bracket sets that have been computed.</param>
        /// <param name="stageConfiguration">The mapping of roundNames -> stageNames.</param>
        /// <param name="config">The MatchingConfiguration to use for building stage groups.</param>
        /// <returns>The collection of stage groups for this league.</returns>
        private static StageGroup[] BuildStageGroups(List<Tournament> tournaments, string[][] bracketSets, Dictionary<string, string> stageConfiguration, MatchingConfiguration config)
        {
            List<StageGroup> groups = new List<StageGroup>();
            Dictionary<string, Bracket> bracketLookup = tournaments.SelectMany(t => t.Brackets).ToDictionary(b => b.BracketId);
            Dictionary<string, Tournament> tournamentLookup = new Dictionary<string, Tournament>();
            foreach (Tournament t in tournaments)
            {
                foreach (Bracket b in t.Brackets)
                {
                    tournamentLookup.TryAdd(b.BracketId, t);
                }
            }

            foreach (string[] brackets in bracketSets)
            {

                int stageIndex = Stage(tournamentLookup[brackets.First()], bracketLookup[brackets.First()], config);
                string stageName = config.stageNames[stageIndex];
                List<StageGroup> roundGroups = new List<StageGroup>();
                int startingRank = 1;

                foreach (string bracket in brackets)
                {
                    Bracket b = bracketLookup[bracket];

                    if (b.TopMetaTeams != null)
                    {
                        StageGroup group = new StageGroup()
                        {
                            Name = stageName,
                            StartingRank = startingRank,
                            Stage = stageName,
                            TeamIds = b.TopMetaTeams.Select(t => t.TeamId).ToArray()
                        };
                        roundGroups.Add(group);
                        startingRank += b.TopMetaTeams.Count;
                    }

                    if (b.MidMetaTeams != null)
                    {
                        StageGroup group = new StageGroup()
                        {
                            Name = stageName,
                            StartingRank = startingRank,
                            Stage = stageName,
                            TeamIds = b.MidMetaTeams.Select(t => t.TeamId).ToArray()
                        };
                        roundGroups.Add(group);
                        startingRank += b.MidMetaTeams.Count;
                    }

                    if (b.BotMetaTeams != null)
                    {
                        StageGroup group = new StageGroup()
                        {
                            Name = stageName,
                            StartingRank = startingRank,
                            Stage = stageName,
                            TeamIds = b.BotMetaTeams.Select(t => t.TeamId).ToArray()
                        };
                        roundGroups.Add(group);
                        startingRank += b.BotMetaTeams.Count;
                    }

                    if (b.TopMetaTeams == null && b.MidMetaTeams == null && b.BotMetaTeams == null)
                    {
                        StageGroup group = new StageGroup()
                        {
                            Name = stageName,
                            StartingRank = startingRank,
                            Stage = stageName,
                            TeamIds = b.Teams.Select(t => t.TeamId).ToArray()
                        };
                        roundGroups.Add(group);
                        startingRank += b.Teams.Count;
                    }
                }

                if (roundGroups.Count > 1)
                {
                    char g = 'A';
                    foreach (StageGroup sg in roundGroups)
                    {
                        sg.Name = $"{sg.Name} {g}";
                        g++;
                    }
                }

                groups.AddRange(roundGroups);
            }

            return groups.ToArray();
        }

        /// <summary>
        /// Builds the bracket sets for a tournament.
        /// </summary>
        /// <param name="tournaments">The filtered list of tournaments.</param>
        /// <param name="config">The MatchingConfiguration to use for building the bracket sets.</param>
        /// <returns>The list of list of bracketIds referenced by the tournaments.</returns>
        private static string[][] BuildBracketSets(List<Tournament> tournaments, MatchingConfiguration config)
        {
            List<Tuple<Tournament, Bracket>> brackets = new List<Tuple<Tournament, Bracket>>();
            foreach (Tournament t in tournaments)
            {
                foreach (Bracket b in t.Brackets)
                {
                    if ((config.bracketBlacklist == null || !config.bracketBlacklist.Contains(b.BracketId))
                        && b.Rounds.Count > 0)
                    {
                        brackets.Add(new Tuple<Tournament, Bracket>(t, b));
                    }
                }
            }

            var stageGroups = brackets.OrderBy(t => Ordering(t.Item1, t.Item2, config)).GroupBy(t => Stage(t.Item1, t.Item2, config)).OrderBy(g => g.Key);
            string[][] bracketSets = stageGroups.Select(s => s.Select(t => t.Item2.BracketId).ToArray()).ToArray();
            return bracketSets;
        }

        /// <summary>
        /// Builds mapping from each round name to the stage name associated.
        /// </summary>
        /// <param name="tournaments">The filtered list of tournaments.</param>
        /// <param name="config">The MatchingConfiguration to buidl the stage configurations.</param>
        /// <returns>The StageConfiguration.</returns>
        private static Dictionary<string, string> BuildStageConfigurations(List<Tournament> tournaments, MatchingConfiguration config)
        {
            Dictionary<string, string> stageConfiguration = new Dictionary<string, string>();
            foreach (Tournament t in tournaments)
            {
                foreach (Bracket b in t.Brackets)
                {
                    int stageIndex = Stage(t, b, config);
                    string stageName = config.stageNames[stageIndex];
                    foreach (BracketRound r in b.Rounds)
                    {
                        if (!stageConfiguration.ContainsKey(r.RoundName))
                        {
                            stageConfiguration[r.RoundName] = stageName;
                        }
                    }
                }
            }

            return stageConfiguration;
        }

        /// <summary>
        /// Gets the stage integer for a given bracket based on the matching configuration.
        /// </summary>
        /// <param name="t">The tournament for a given bracket.</param>
        /// <param name="b">The bracket.</param>
        /// <param name="config">The MatchingConfiguration to resolve to a stage index.</param>
        /// <returns>The stage for this tournament/bracket pair.</returns>
        private static int Stage(Tournament t, Bracket b, MatchingConfiguration config)
        {
            if (config.stageKeywords == null)
            {
                return 0;
            }

            foreach (KeyValuePair<string, int> kvp in config.stageKeywords)
            {
                if (Contains(t, b, kvp.Key))
                {
                    return kvp.Value;
                }
            }

            return 0;
        }

        /// <summary>
        /// Gets an ordering offset within a stage for the given tournament and bracket based on the MatchingConfiguration.
        /// </summary>
        /// <param name="t">The tournament for a given bracket.</param>
        /// <param name="b">The bracket.</param>
        /// <param name="config">The MatchingConfiguraiton to resolve to ordering index.</param>
        /// <returns>An offset for where this bracket should go in the ordering.</returns>
        private static int Ordering(Tournament t, Bracket b, MatchingConfiguration config)
        {
            if (config.orderKeywords == null)
            {
                return 0;
            }

            foreach (KeyValuePair<string, int> kvp in config.orderKeywords)
            {
                if (Contains(t, b, kvp.Key))
                {
                    return kvp.Value;
                }
            }

            return 0;
        }

        /// <summary>
        /// Checks if the tournament name or bracket name contains the target string.
        /// </summary>
        /// <param name="t">The tournament to check.</param>
        /// <param name="b">The bracket to check.</param>
        /// <param name="target">The target string.</param>
        /// <returns></returns>
        private static bool Contains(Tournament t, Bracket b, string target)
        {
            return (t.TournamentName.Contains(target, StringComparison.OrdinalIgnoreCase) || b.Name.Contains(target, StringComparison.OrdinalIgnoreCase));
        }
    }
}
