using Newtonsoft.Json.Linq;
using PlayCEASharp.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayCEASharp.RequestManagement
{
    internal static class Marshaller
    {
        internal static Tournament Tournament(JToken tournamentToken) {
            Tournament tournament = ResourceCache.GetTournament((string)tournamentToken["tmid"]);
            tournament.TournamentName = (string)tournamentToken["name"];

            if (tournamentToken["current"] != null)
            {
                tournament.Current = (bool)tournamentToken["current"];
            }

            if (tournamentToken["live"] != null)
            {
                tournament.Live = (bool)tournamentToken["live"];
            }

            if (tournamentToken["msg"] != null)
            {
                tournament.Message = (string)tournamentToken["msg"];
            }

            if (tournamentToken["meta"] != null)
            {
                tournament.Metadata = tournamentToken["meta"];
            }
            tournament.GameId = (string)tournamentToken["game"]["id"];
            tournament.GameName = (string)tournamentToken["game"]["name"];
            tournament.SeasonLeague = (string)tournamentToken["sn"]["l"];
            tournament.SeasonSeason = (string)tournamentToken["sn"]["s"];
            tournament.SeasonYear = (string)tournamentToken["sn"]["y"];

            foreach (JToken bracketToken in tournamentToken["bs"])
            {
                Bracket b = ResourceCache.GetBracket((string)bracketToken["bid"]);
                b.Name = (string)bracketToken["name"];
                if (!tournament.Brackets.Contains(b))
                {
                    tournament.Brackets.Add(b);
                }
            }

            foreach (JToken teamToken in tournamentToken["ts"])
            {
                Team team = ResourceCache.GetTeam((string)teamToken["tid"]);
                if (!tournament.Teams.Contains(team))
                {
                    tournament.Teams.Add(team);
                }
            }

            return tournament;
        }

        internal static Bracket Bracket(JToken bracketToken)
        {
            Bracket bracket1 = ResourceCache.GetBracket((string)bracketToken["bid"]);
            bracket1.Name = (string)bracketToken["name"];
            Bracket bracket = bracket1;
            foreach (JToken token in bracketToken["rounds"])
            {
                BracketRound round = BracketRound(token);
                if (!bracket.Rounds.Contains(round))
                {
                    bracket.Rounds.Add(round);
                }
            }
            foreach (JToken token2 in bracketToken["ts"])
            {
                Team team = Team(token2);
                if (!bracket.Teams.Contains(team))
                {
                    bracket.Teams.Add(team);
                }
            }

            if (bracketToken["meta"] != null)
            {
                string firstRoundId = bracket.Rounds.First().RoundId;
                if (bracketToken["meta"][firstRoundId] != null && bracketToken["meta"][firstRoundId]["buckets"] != null)
                {
                    JToken topBucket = bracketToken["meta"][firstRoundId]["buckets"]["top"];
                    bracket.TopMetaTeams = ExtractTeamsFromBucket(topBucket);
                    JToken midBucket = bracketToken["meta"][firstRoundId]["buckets"]["mid"];
                    bracket.MidMetaTeams = ExtractTeamsFromBucket(midBucket);
                    JToken botBucket = bracketToken["meta"][firstRoundId]["buckets"]["bot"];
                    bracket.BotMetaTeams = ExtractTeamsFromBucket(botBucket);
                }
            }

            return bracket;
        }

        private static List<Team> ExtractTeamsFromBucket(JToken bucketToken)
        {
            List<Team> teams = new List<Team>();

            foreach (JToken teamToken in bucketToken)
            {
                Team t = ResourceCache.GetTeam((string)teamToken["tid"]);
                teams.Add(t);
            }

            return teams;
        }

        internal static BracketRound BracketRound(JToken roundToken)
        {
            BracketRound bracketRound = ResourceCache.GetBracketRound((string)roundToken["rid"]);
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

        internal static Game Game(JToken gameToken)
        {
            Game game1 = new Game((string)gameToken["gid"]);
            game1.HomeTeam = ResourceCache.GetTeam((string)gameToken["ts"][(int)0]["tid"]);
            game1.HomeScore = (int)gameToken["ts"][(int)0]["rs"];
            game1.AwayTeam = ResourceCache.GetTeam((string)gameToken["ts"][(int)1]["tid"]);
            game1.AwayScore = (int)gameToken["ts"][(int)1]["rs"];
            return game1;
        }

        internal static MatchResult Match(JToken matchToken, BracketRound optionalBracketRound = null)
        {
            MatchResult result2;
            if (object.ReferenceEquals(matchToken["ts"].First, matchToken["ts"].Last))
            {
                MatchResult result1 = new MatchResult((string)matchToken["mid"]);
                result1.Round = (int)matchToken["rnd"];
                result1.HomeTeam = ResourceCache.GetTeam((string)matchToken["ts"][(int)0]["tid"]);
                result1.AwayTeam = null;
                result1.Bye = true;
                result2 = result1;
            }
            else
            {
                MatchResult result3 = new MatchResult((string)matchToken["mid"]);
                result3.Round = (int)matchToken["rnd"];
                result3.HomeTeam = ResourceCache.GetTeam((string)matchToken["ts"][(int)0]["tid"]);
                result3.AwayTeam = ResourceCache.GetTeam((string)matchToken["ts"][(int)1]["tid"]);
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

        internal static Player Player(JToken playerToken)
        {
            Player player1 = ResourceCache.GetPlayer((string)playerToken["uid"]);
            player1.DisplayName = (string)playerToken["dn"];
            player1.DiscordId = (string)playerToken["ddn"];
            player1.DiscordUID = (string)playerToken["uid"];
            player1.PictureURL = (string)playerToken["ico"];
            player1.Captain = (bool)playerToken["captain"];
            return player1;
        }

        internal static Team Team(JToken teamToken)
        {
            Team team = ResourceCache.GetTeam((string)teamToken["tid"]);
            team.Name = (string)teamToken["dn"];
            team.Org = (string)teamToken["org"];
            team.ImageURL = (string)teamToken["ico"];
            if (teamToken["r"] != null)
            {
                team.Rank = (int)teamToken["r"];
            }
            if (teamToken["mbr"] != null)
            {
                foreach (JToken token in teamToken["mbr"])
                {
                    Player player = Player(token);
                    if (!team.Players.Contains(player))
                    {
                        team.Players.Add(player);
                    }
                }
            }
            return team;
        }
    }
}
