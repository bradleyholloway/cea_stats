using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEASharp.DataModel
{
    public class League
    {
        private List<BracketSet> bracketSets;

        internal League(List<BracketSet> bracketSets)
        {
            if (bracketSets.Count > 0)
            {
                this.bracketSets = bracketSets;
                this.Bracket = bracketSets.Last();
                this.PlayerDiscordLookup = new Dictionary<string, Team>();
                foreach (Team team in this.Bracket.Teams)
                {
                    foreach (Player player in team.Players)
                    {
                        this.PlayerDiscordLookup[player.DiscordId] =  team;
                    }
                }
                this.NextMatchLookup = new Dictionary<Team, MatchResult>();

                foreach (MatchResult result in this.Bracket.Rounds.Last().SelectMany(r => r.Matches))
                {
                    this.NextMatchLookup[result.HomeTeam] = result;
                    if (result.AwayTeam != null)
                    {
                        this.NextMatchLookup[result.AwayTeam] = result;
                    }
                }
            }
        }

        public Dictionary<string, Team> PlayerDiscordLookup { get; }

        public Dictionary<Team, MatchResult> NextMatchLookup { get; }

        public BracketSet Bracket { get; }

        public List<BracketSet> Brackets =>
            this.bracketSets;

    }
}
