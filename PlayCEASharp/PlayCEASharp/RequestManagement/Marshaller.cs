﻿using PlayCEASharp.Configuration;
using PlayCEASharp.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace PlayCEASharp.RequestManagement
{
    /// <summary>
    /// Class for marshalling responses from PlayCEA into PlayCEASharp objects.
    /// </summary>
    internal static class Marshaller
    {
        /// <summary>
        /// Transforms a json tournament into a Tournament.
        /// </summary>
        /// <param name="tournamentToken">the json for the tournament.</param>
        /// <returns>A hydrated Tournament</returns>
        internal static Tournament Tournament(JsonNode tournamentToken)
        {
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

            // Some seasons are in the database as strings, while others are numbers.
            // We default trying to decode as a string first, and if that is invalid we attempt as a number.
            try
            {
                tournament.SeasonYear = (string)tournamentToken["sn"]["y"];
            }
            catch (InvalidOperationException) {
                tournament.SeasonYear = ((int)tournamentToken["sn"]["y"]).ToString();
            }

            string regularSeasonBracketId = (string)tournamentToken["sbkt"]["reg"];
            if (!string.IsNullOrEmpty(regularSeasonBracketId))
            {
                Bracket reg = ResourceCache.GetBracket(regularSeasonBracketId);
                tournament.RegularSeason = reg;
                reg.Tournament = tournament;
                if (!tournament.Brackets.Contains(reg))
                {
                    tournament.Brackets.Add(reg);
                }
            }

            string playoffBracketId = (string)tournamentToken["sbkt"]["po"];
            if (!string.IsNullOrEmpty(playoffBracketId))
            {
                Bracket playoffs = ResourceCache.GetBracket(playoffBracketId);
                playoffs.Tournament = tournament;
                tournament.Playoffs = playoffs;
                if (!tournament.Brackets.Contains(playoffs))
                {
                    tournament.Brackets.Add(playoffs);
                }
            }

            foreach (JsonNode bracketToken in tournamentToken["bs"].AsArray())
            {
                Bracket b = ResourceCache.GetBracket((string)bracketToken["bid"]);
                b.Name = (string)bracketToken["name"];
            }

            foreach (JsonNode teamToken in tournamentToken["ts"].AsArray())
            {
                Team team = ResourceCache.GetTeam((string)teamToken["tid"]);
                if (!tournament.Teams.Contains(team))
                {
                    tournament.Teams.Add(team);
                }
            }

            return tournament;
        }

        /// <summary>
        /// Transforms the json bracket into a Bracket.
        /// </summary>
        /// <param name="bracketToken">The json returned from PlayCEA.</param>
        /// <param name="tc">The Tournament Configuration used for this Bracket.</param>
        /// <returns>A hydrated Bracket.</returns>
        internal static Bracket Bracket(JsonNode bracketToken, TournamentConfiguration tc)
        {
            Bracket bracket = ResourceCache.GetBracket((string)bracketToken["bid"]);
            bracket.Name = (string)bracketToken["name"];
            bracket.Game = (string)bracketToken["game"];
            foreach (JsonNode token in bracketToken["rounds"].AsArray())
            {
                BracketRound round = BracketRound(token, bracket);
                if (!bracket.Rounds.Contains(round))
                {
                    bracket.Rounds.Add(round);
                }
            }
            foreach (JsonNode token2 in bracketToken["ts"].AsArray())
            {
                Team team = Team(token2, tc);
                if (!bracket.Teams.Contains(team))
                {
                    bracket.Teams.Add(team);
                }
            }

            if (bracketToken["meta"] != null && bracket.Rounds.Count > 0)
            {
                string firstRoundId = bracket.Rounds.First().RoundId;
                if (bracketToken["meta"][firstRoundId] != null && bracketToken["meta"][firstRoundId]["buckets"] != null)
                {
                    JsonNode topBucket = bracketToken["meta"][firstRoundId]["buckets"]["top"];
                    bracket.TopMetaTeams = ExtractTeamsFromBucket(topBucket);
                    JsonNode midBucket = bracketToken["meta"][firstRoundId]["buckets"]["mid"];
                    bracket.MidMetaTeams = ExtractTeamsFromBucket(midBucket);
                    JsonNode botBucket = bracketToken["meta"][firstRoundId]["buckets"]["bot"];
                    bracket.BotMetaTeams = ExtractTeamsFromBucket(botBucket);
                }
            }

            return bracket;
        }

        /// <summary>
        /// Gets a list of teams from a bucket in metadata.
        /// </summary>
        /// <param name="bucketToken">The meatdata bucket token.</param>
        /// <returns>List of teams.</returns>
        private static List<Team> ExtractTeamsFromBucket(JsonNode bucketToken)
        {
            List<Team> teams = new List<Team>();

            foreach (JsonNode teamToken in bucketToken.AsArray())
            {
                Team t = ResourceCache.GetTeam((string)teamToken["tid"]);
                teams.Add(t);
            }

            return teams;
        }

        /// <summary>
        /// Transforms a json bracket round into a BracketRound.
        /// </summary>
        /// <param name="roundToken">the json of a bracketround from PlayCEA.</param>
        /// <param name="bracket">the bracket parent of this round.</param>
        /// <returns>A hydrated BracketRound.</returns>
        internal static BracketRound BracketRound(JsonNode roundToken, Bracket bracket)
        {
            BracketRound bracketRound = ResourceCache.GetBracketRound((string)roundToken["rid"]);
            bracketRound.RoundName = (string)roundToken["roundName"];
            bracketRound.Complete = (bool)roundToken["complete"];
            bracketRound.GameCount = (int)roundToken["gameCount"];
            bracketRound.Matches.Clear();
            foreach (JsonNode token in roundToken["matches"].AsArray())
            {
                MatchResult item = Match(token, bracketRound);
                bracketRound.Matches.Add(item);
            }

            bracketRound.Bracket = bracket;
            return bracketRound;
        }

        /// <summary>
        /// Transforms the json game token into a Game.
        /// </summary>
        /// <param name="gameToken">json representation of a game.</param>
        /// <returns>A hydrated Game.</returns>
        internal static Game Game(JsonNode gameToken)
        {
            Game game = new Game((string)gameToken["gid"]);
            JsonNode tid = gameToken["ts"][(int)0]["tid"];
            game.HomeTeam = ResourceCache.GetTeam(ExtractTid(tid));
            game.HomeScore = (int)gameToken["ts"][(int)0]["rs"];
            tid = gameToken["ts"][(int)1]["tid"];
            game.AwayTeam = ResourceCache.GetTeam(ExtractTid(tid));
            game.AwayScore = (int)gameToken["ts"][(int)1]["rs"];
            return game;
        }

        /// <summary>
        /// Transforms the json representation of a match into a MatchResult.
        /// </summary>
        /// <param name="matchToken">the json representation from PlayCEA.</param>
        /// <param name="optionalBracketRound">The BracketRound to provide a backlink for round rankings.</param>
        /// <returns>A MatchResult for the given match.</returns>
        internal static MatchResult Match(JsonNode matchToken, BracketRound optionalBracketRound = null)
        {
            MatchResult result;
            result = new MatchResult((string)matchToken["mid"]);
            result.Round = (int)matchToken["rnd"];
            result.GameId = (string)matchToken["game"];
            result.BracketRound = optionalBracketRound ?? ResourceCache.GetBracketRound((string)matchToken["bid"]);
            result.Completed = (matchToken["complete"] == null) ? result.BracketRound.Complete : (bool)matchToken["complete"];
            if (object.ReferenceEquals(matchToken["ts"].AsArray().First(), matchToken["ts"].AsArray().Last()))
            {
                result.HomeTeam = ResourceCache.GetTeam((string)matchToken["ts"][(int)0]["tid"]);
                result.AwayTeam = null;
                result.Bye = true;
            }
            else
            {
                result.HomeTeam = ResourceCache.GetTeam((string)matchToken["ts"][(int)0]["tid"]);
                result.AwayTeam = ResourceCache.GetTeam((string)matchToken["ts"][(int)1]["tid"]);
                if ((optionalBracketRound != null) && (matchToken["ts"][(int)0]["rank"] != null))
                {
                    result.HomeTeam.FixedRoundRanking[optionalBracketRound] = (int)matchToken["ts"][(int)0]["rank"];
                    result.AwayTeam.FixedRoundRanking[optionalBracketRound] = (int)matchToken["ts"][(int)1]["rank"];
                }
                JsonNode token = matchToken["gs"];
                result.Games = new List<Game>();
                foreach (JsonNode token2 in token.AsArray())
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
            }

            return result;
        }

        /// <summary>
        /// Transforms the json of a player into a Player.
        /// </summary>
        /// <param name="playerToken">the json representation of the player.</param>
        /// <returns>A hydrated Player.</returns>
        internal static Player Player(JsonNode playerToken)
        {
            Player player = new Player((string)playerToken["uid"]);
            player.DisplayName = (string)playerToken["dn"];
            player.DiscordId = (string)playerToken["ddn"];
            bool validDiscordId = ulong.TryParse((string)playerToken["uid"], out ulong discordUID);
            if (!validDiscordId)
            {
                return null;
            }

            player.DiscordUID = discordUID;
            player.PictureURL = (string)playerToken["ico"];
            player.Captain = (bool)playerToken["captain"];
            return player;
        }

        /// <summary>
        /// Transforms the json representation of a team into a Team.
        /// </summary>
        /// <param name="teamToken">the json representation of the team.</param>
        /// <param name="tc">The TournamentConfiguration used for the context of this team.</param>
        /// <returns>A hydrated Team.</returns>
        internal static Team Team(JsonNode teamToken, TournamentConfiguration tc = null)
        {
            Team team = ResourceCache.GetTeam((string)teamToken["tid"]);
            team.NameConfiguration = tc?.namingConfig ?? NamingConfiguration.DefaultInstance;
            team.Name = (string)teamToken["dn"];
            team.Org = (string)teamToken["org"];
            team.ImageURL = (string)teamToken["ico"];
            if (teamToken["r"] != null)
            {
                team.Rank = (int)teamToken["r"];
            }
            if (teamToken["mbr"] != null)
            {
                List<Player> players = new List<Player>();
                foreach (JsonNode token in teamToken["mbr"].AsArray())
                {
                    Player player = Player(token);
                    if (player != null && !players.Contains(player))
                    {
                        players.Add(player);
                    }
                }

                team.Players = players;
            }

            if (teamToken["meta"] != null && teamToken["meta"]["charity"] != null)
            {
                team.Charity = (string)teamToken["meta"]["charity"];
            }

            return team;
        }

        private static string ExtractTid(JsonNode tidToken)
        {
            try
            {
                return (string)tidToken;
            }
            catch (ArgumentException)
            {
                return (string)tidToken["tid"];
            }
        }
    }
}
