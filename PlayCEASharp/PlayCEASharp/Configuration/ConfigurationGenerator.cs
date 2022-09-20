using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PlayCEASharp.DataModel;

namespace PlayCEASharp.Configuration
{
    public class ConfigurationGenerator
    {
        public static BracketConfiguration GenerateConfiguration(List<Tournament> allTournaments)
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

        public static List<Tournament> MatchingTournaments(List<Tournament> allTournaments)
        {
            MatchingConfiguration c = ConfigurationManager.MatchingConfiguration;
            return allTournaments.Where(t => t.GameId.Equals(c.gameId) && t.SeasonYear.Equals(c.year) && t.SeasonLeague.Equals(c.league) && t.SeasonSeason.Equals(c.season)).ToList();
        }

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

        private static bool Contains(Tournament t, Bracket b, string target)
        {
            return (t.TournamentName.Contains(target, StringComparison.OrdinalIgnoreCase) || b.Name.Contains(target, StringComparison.OrdinalIgnoreCase));
        }
    }
}
