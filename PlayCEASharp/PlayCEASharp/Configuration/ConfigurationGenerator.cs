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
        /// <returns></returns>
        internal static BracketConfiguration GenerateConfiguration(List<Tournament> allTournaments)
        {
            List<Tournament> tournaments = MatchingTournaments(allTournaments);

            Dictionary<string, string> stageConfiguration = BuildStageConfigurations(tournaments);
            string[][] bracketSets = BuildBracketSets(tournaments);
            StageGroup[] stageGroups = BuildStageGroups(tournaments, bracketSets, stageConfiguration);
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
        /// <returns>New list of only tournaments which match the configuration.</returns>
        internal static List<Tournament> MatchingTournaments(List<Tournament> allTournaments)
        {
            MatchingConfiguration c = ConfigurationManager.MatchingConfiguration;
            return allTournaments.Where(t => t.GameId.Equals(c.gameId) && t.SeasonYear.Equals(c.year) && t.SeasonLeague.Equals(c.league) && t.SeasonSeason.Equals(c.season)).ToList();
        }

        /// <summary>
        /// Builds the collection of stage groups.
        /// </summary>
        /// <param name="tournaments">The filtered list of relevant tournaments.</param>
        /// <param name="bracketSets">The bracket sets that have been computed.</param>
        /// <param name="stageConfiguration">The mapping of roundNames -> stageNames.</param>
        /// <returns>The collection of stage groups for this league.</returns>
        private static StageGroup[] BuildStageGroups(List<Tournament> tournaments, string[][] bracketSets, Dictionary<string, string> stageConfiguration)
        {
            List<StageGroup> groups = new List<StageGroup>();
            Dictionary<string, Bracket> bracketLookup = tournaments.SelectMany(t => t.Brackets).ToDictionary(b => b.BracketId);

            int stageIndex = -1;
            foreach (string[] brackets in bracketSets)
            {
                string stageName = ConfigurationManager.MatchingConfiguration.stageNames[stageIndex];
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
                
                stageIndex++;
            }

            return groups.ToArray();
        }

        /// <summary>
        /// Builds the bracket sets for a tournament.
        /// </summary>
        /// <param name="tournaments">The filtered list of tournaments.</param>
        /// <returns>The list of list of bracketIds referenced by the tournaments.</returns>
        private static string[][] BuildBracketSets(List<Tournament> tournaments)
        {
            List<Tuple<Tournament, Bracket>> brackets = new List<Tuple<Tournament, Bracket>>();
            foreach (Tournament t in tournaments)
            {
                foreach (Bracket b in t.Brackets)
                {
                    brackets.Add(new Tuple<Tournament, Bracket>(t, b));
                }
            }

            var stageGroups = brackets.OrderBy(t => Ordering(t.Item1, t.Item2)).GroupBy(t => Stage(t.Item1, t.Item2)).OrderBy(g => g.Key);
            string[][] bracketSets = stageGroups.Select(s => s.Select(t => t.Item2.BracketId).ToArray()).ToArray();
            return bracketSets;
        }

        /// <summary>
        /// Builds mapping from each round name to the stage name associated.
        /// </summary>
        /// <param name="tournaments">The filtered list of tournaments.</param>
        /// <returns>The StageConfiguration.</returns>
        private static Dictionary<string, string> BuildStageConfigurations(List<Tournament> tournaments)
        {
            Dictionary<string, string> stageConfiguration = new Dictionary<string, string>();
            foreach (Tournament t in tournaments)
            {
                foreach (Bracket b in t.Brackets)
                {
                    int stageIndex = Stage(t, b);
                    string stageName = ConfigurationManager.MatchingConfiguration.stageNames[stageIndex];
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
        /// <returns>The stage for this tournament/bracket pair.</returns>
        private static int Stage(Tournament t, Bracket b)
        {
            foreach (KeyValuePair<string, int> kvp in ConfigurationManager.MatchingConfiguration.stageKeywords)
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
        /// <returns>An offset for where this bracket should go in the ordering.</returns>
        private static int Ordering(Tournament t, Bracket b)
        {
            foreach (KeyValuePair<string, int> kvp in ConfigurationManager.MatchingConfiguration.orderKeywords)
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
