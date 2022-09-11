using Newtonsoft.Json.Linq;
using PlayCEA.RLClient.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEA.RLClient.RequestManagement
{
    public static class Marshaller
    {
        public static Bracket Bracket(JToken bracketToken)
        {
            Bracket bracket1 = new Bracket((string)bracketToken["bid"]);
            bracket1.Name = (string)bracketToken["name"];
            Bracket bracket = bracket1;
            foreach (JToken token in bracketToken["rounds"])
            {
                bracket.Rounds.Add(BracketRound(token));
            }
            foreach (JToken token2 in bracketToken["ts"])
            {
                bracket.Teams.Add(Team(token2));
            }
            return bracket;
        }

        public static BracketRound BracketRound(JToken roundToken)
        {
            BracketRound bracketRound = BracketRoundCache.GetBracketRound((string)roundToken["rid"]);
            bracketRound.RoundName = (string)roundToken["roundName"];
            bracketRound.Complete = (bool)roundToken["complete"];
            bracketRound.GameCount = (int)roundToken["gameCount"];
            bracketRound.Matches.Clear();
            foreach (JToken token in roundToken["matches"])
            {
                MatchResult item = Match(token, bracketRound);
                bracketRound.Matches.Add(item);
            }
            return bracketRound;
        }

        public static Game Game(JToken gameToken)
        {
            Game game1 = new Game((string)gameToken["gid"]);
            game1.HomeTeam = TeamCache.GetTeam((string)gameToken["ts"][(int)0]["tid"]);
            game1.HomeScore = (int)gameToken["ts"][(int)0]["rs"];
            game1.AwayTeam = TeamCache.GetTeam((string)gameToken["ts"][(int)1]["tid"]);
            game1.AwayScore = (int)gameToken["ts"][(int)1]["rs"];
            return game1;
        }

        public static MatchResult Match(JToken matchToken, BracketRound optionalBracketRound = null)
        {
            MatchResult result2;
            if (object.ReferenceEquals(matchToken["ts"].First, matchToken["ts"].Last))
            {
                MatchResult result1 = new MatchResult();
                result1.MatchId = (string)matchToken["mid"];
                result1.Round = (int)matchToken["rnd"];
                result1.HomeTeam = TeamCache.GetTeam((string)matchToken["ts"][(int)0]["tid"]);
                result1.AwayTeam = null;
                result1.Bye = true;
                result2 = result1;
            }
            else
            {
                MatchResult result3 = new MatchResult();
                result3.MatchId = (string)matchToken["mid"];
                result3.Round = (int)matchToken["rnd"];
                result3.HomeTeam = TeamCache.GetTeam((string)matchToken["ts"][(int)0]["tid"]);
                result3.AwayTeam = TeamCache.GetTeam((string)matchToken["ts"][(int)1]["tid"]);
                MatchResult result = result3;
                if ((optionalBracketRound != null) && (matchToken["ts"][(int)0]["rank"] != null))
                {
                    result.HomeTeam.RoundRanking[optionalBracketRound] = (int)matchToken["ts"][(int)0]["rank"];
                    result.AwayTeam.RoundRanking[optionalBracketRound] = (int)matchToken["ts"][(int)1]["rank"];
                }
                JToken token = matchToken["gs"];
                result.Games = new List<Game>();
                foreach (JToken token2 in token)
                {
                    Game item = Game(token2);
                    if (item.HomeScore > item.AwayScore)
                    {
                        result.HomeGamesWon++;
                    }
                    else if (item.AwayScore > item.HomeScore)
                    {
                        result.AwayGamesWon++;
                    }
                    result.HomeGoals += item.HomeScore;
                    result.AwayGoals += item.AwayScore;
                    result.Games.Add(item);
                }
                result2 = result;
            }
            return result2;
        }

        public static Player Player(JToken playerToken)
        {
            Player player1 = new Player((string)playerToken["uid"]);
            player1.DisplayName = (string)playerToken["dn"];
            player1.DiscordId = (string)playerToken["ddn"];
            player1.Captain = (bool)playerToken["captain"];
            return player1;
        }

        public static Team Team(JToken teamToken)
        {
            Team team = TeamCache.GetTeam((string)teamToken["tid"]);
            team.Name = (string)teamToken["dn"];
            team.Org = (string)teamToken["org"];
            team.ImageURL = (string)teamToken["ico"];
            if (teamToken["r"] != null)
            {
                team.Rank = (int)teamToken["r"];
            }
            if ((team.Players.Count == 0) && (teamToken["mbr"] != null))
            {
                foreach (JToken token in teamToken["mbr"])
                {
                    team.Players.Add(Player(token));
                }
            }
            return team;
        }
    }
}
