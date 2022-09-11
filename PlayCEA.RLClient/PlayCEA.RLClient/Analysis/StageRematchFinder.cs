using PlayCEA.RLClient.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEA.RLClient.Analysis
{
    public class StageRematchFinder
    {
        public static string FindRematches(Bracket bracket, string stage)
        {
            StringBuilder sb = new StringBuilder();
            FindRematchesCore(bracket, stage, sb);
            if (sb.Length == 0)
            {
                sb.AppendLine("No rematches found for the current round.");
            }
            return sb.ToString();
        }

        public static string FindRematches(BracketSet bracketSet, string stage)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Bracket bracket in bracketSet.Brackets)
            {
                FindRematchesCore(bracket, stage, sb);
            }
            if (sb.Length == 0)
            {
                sb.AppendLine("No rematches found for the current round.");
            }
            return sb.ToString();
        }

        private static void FindRematchesCore(Bracket bracket, string stage, StringBuilder sb)
        {
            Dictionary<Team, HashSet<Team>> dictionary = new Dictionary<Team, HashSet<Team>>();
            foreach (Team team in bracket.Teams)
            {
                dictionary.Add(team, new HashSet<Team>());
            }
            foreach (BracketRound round in Enumerable.Take<BracketRound>((IEnumerable<BracketRound>)bracket.Rounds, bracket.Rounds.Count - 1))
            {
                string str = StageMatcher.Lookup(round.RoundName);
                if (str.Equals(stage))
                {
                    foreach (MatchResult result in round.Matches)
                    {
                        dictionary[result.HomeTeam].Add(result.AwayTeam);
                        dictionary[result.AwayTeam].Add(result.HomeTeam);
                    }
                }
            }
            foreach (MatchResult result2 in Enumerable.Last<BracketRound>((IEnumerable<BracketRound>)bracket.Rounds).Matches)
            {
                if (dictionary[result2.HomeTeam].Contains(result2.AwayTeam))
                {
                    sb.AppendLine($"Rematch identified between {result2.HomeTeam} and {result2.AwayTeam}.");
                }
            }
        }
    }
}
