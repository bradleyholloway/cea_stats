using Newtonsoft.Json.Linq;
using PlayCEASharp.Configuration;
using PlayCEASharp.DataModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PlayCEASharp.RequestManagement
{
    /// <summary>
    /// Wraps requests to the api endpoints for PlayCEA.
    /// </summary>
    public class RequestManager
    {
        /// <summary>
        /// The HttpClient to use when issuing requests.
        /// </summary>
        private readonly HttpClient client = new HttpClient();

        /// <summary>
        /// The base api endpoint for PlayCEA.
        /// </summary>
        private const string apiEndpoint = "https://1ebv8yx4pa.execute-api.us-east-1.amazonaws.com/prod";

        /// <summary>
        /// The endpoint override.
        /// </summary>
        private string endpoint;

        /// <summary>
        /// Creates a new request manager.
        /// </summary>
        public RequestManager(string? optionalEndpoint)
        {
            this.endpoint = optionalEndpoint ?? apiEndpoint;
            this.client.DefaultRequestHeaders.Accept.Clear();
            this.client.DefaultRequestHeaders.Add("User-Agent", "Play CEA Stats Collector");
        }

        /// <summary>
        /// Loads all brackets referenced by a collection of tournaments.
        /// </summary>
        /// <param name="tournaments">Collection of tournaments to populate data for.</param>
        /// <returns>A task to load all of the data.</returns>
        internal async Task LoadBrackets(List<Tournament> tournaments, TournamentConfiguration tc)
        {
            List<Task> allTasks = new List<Task>();
            foreach(Tournament t in tournaments)
            {
                foreach(Bracket b in t.Brackets)
                {
                    allTasks.Add(this.GetBracket(b.BracketId, tc));
                }
            }

            await Task.WhenAll(allTasks);
        }

        /// <summary>
        /// Gets a bracket from PlayCEA.
        /// </summary>
        /// <param name="bracketId">The bracket id to read.</param>
        /// <returns>The fully populated Bracket.</returns>
        internal async Task<Bracket> GetBracket(string bracketId, TournamentConfiguration tc)
        {
            string content = await this.GetStringWithRetryAsync($"{endpoint}/brackets/{bracketId}");
            JObject jObject = JObject.Parse(content);
            Bracket bracket = Marshaller.Bracket(jObject["data"][0], tc);
            return bracket;
        }

        /// <summary>
        /// Gets a specific match result from PlayCEA.
        /// </summary>
        /// <param name="matchId">The match id.</param>
        /// <returns>The most recent info for the match.</returns>
        public async Task<MatchResult> GetMatchResult(string matchId)
        {
            string content = await this.GetStringWithRetryAsync($"{endpoint}/matches/{matchId}");
            JObject jObject = JObject.Parse(content);
            MatchResult match = Marshaller.Match(jObject["data"]);
            return match;
        }

        /// <summary>
        /// Gets a specific team from PlayCEA.
        /// </summary>
        /// <param name="teamId">The team id</param>
        /// <returns>The updated team.</returns>
        public async Task<Team> GetTeam(string teamId, TournamentConfiguration tc)
        {
            string content = await this.GetStringWithRetryAsync($"{endpoint}/teams/{teamId}");
            JObject jObject = JObject.Parse(content);
            Team team = Marshaller.Team(jObject["data"][0], tc);
            return team;
        }

        /// <summary>
        /// Reports scores for a match using a given discord bearer token.
        /// </summary>
        /// <param name="match">The match to report scores for.</param>
        /// <param name="discordBearerToken">Bearer token to use to update.</param>
        public async Task ReportScores(MatchResult match, string discordBearerToken)
        {
            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}/matches/{match.MatchId}/scores");
            httpRequest.Headers.Add("authorization", $"bearer {discordBearerToken}");
            StringContent content = new StringContent(MatchResultToScoresUpdate(match));
            httpRequest.Content = content;
            await this.client.SendAsync(httpRequest);
        }

        private string MatchResultToScoresUpdate(MatchResult match)
        {
            /*
             * {"games":[
             *  {"gid":"ogEXlcv8sT","scores":[{"tid":"ML0gzWQDdh","score":1},{"tid":"US78M7hQXt","score":0}]},
             *  {"gid":"SyREj3hAWD","scores":[{"tid":"ML0gzWQDdh","score":0},{"tid":"US78M7hQXt","score":2}]},
             *  {"gid":"5Tj58m2kqK","scores":[{"tid":"ML0gzWQDdh","score":3},{"tid":"US78M7hQXt","score":0}]},
             *  {"gid":"4BiEpSQ_Pe","scores":[{"tid":"ML0gzWQDdh","score":0},{"tid":"US78M7hQXt","score":4}]},
             *  {"gid":"k9lL7_0Zjo","scores":[{"tid":"ML0gzWQDdh","score":5},{"tid":"US78M7hQXt","score":0}]}]}
             */
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"games\":[");

            int gameCount = match.Games.Count;
            int gameNumber = 1;
            foreach (Game game in match.Games)
            {
                sb.Append($"{{\"gid\":\"{game.GameId}\",\"scores\":[{{\"tid\":\"{game.HomeTeam.TeamId}\",\"score\":{game.HomeScore}}},{{\"tid\":\"{game.AwayTeam.TeamId}\",\"score\":{game.AwayScore}}}]}}");
                if (gameNumber != gameCount)
                {
                    sb.Append(",");
                }

                gameNumber++;
            }

            sb.Append("]}");
            return sb.ToString();
        }

        /// <summary>
        /// Gets all tournaments from PlayCEA.
        /// </summary>
        /// <returns>Collection of all Tournaments from PlayCEA.</returns>
        internal async Task<List<Tournament>> GetTournaments(TournamentConfiguration tc) {
            string content = await this.GetStringWithRetryAsync($"{endpoint}/tournaments");
            JObject jObject = JObject.Parse(content);
            List<Tournament> tournaments = new List<Tournament>();
            foreach (JToken t in jObject["data"]) {
                Tournament tournament = Marshaller.Tournament(t, tc);
                tournaments.Add(tournament);
            }
            return tournaments;
        }

        /// <summary>
        /// Gets a specific tournament from PlayCEA.
        /// </summary>
        /// <param name="tournamentId">The tournament id.</param>
        /// <returns>The updated tournament.</returns>
        internal async Task<Tournament> GetTournament(string tournamentId, TournamentConfiguration tc) {
            string content = await this.GetStringWithRetryAsync($"{endpoint}/tournaments/{tournamentId}");
            JObject jObject = JObject.Parse(content);
            Tournament tournament = Marshaller.Tournament(jObject["data"], tc);
            return tournament;
        }

        /// <summary>
        /// Updates all teams from a collection of teams.
        /// </summary>
        /// <param name="teams">The list of teams to update.</param>
        /// <returns>Task for updating the teams in parallel.</returns>
        internal async Task UpdateAllTeams(List<Team> teams, TournamentConfiguration tc)
        {
            List<Task> allTasks = new List<Task>();
            foreach (Team team in teams)
            {
                allTasks.Add(this.UpdateTeamDetails(team, tc));
            }

            await Task.WhenAll(allTasks);
        }

        /// <summary>
        /// Updates the details for a single team.
        /// </summary>
        /// <param name="team">The team to update.</param>
        /// <returns>Task for updating the team.</returns>
        internal async Task UpdateTeamDetails(Team team, TournamentConfiguration tc)
        {
            try
            {
                await this.GetTeam(team.TeamId, tc);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }

        private async Task<string> GetStringWithRetryAsync(string request)
        {
            HttpResponseMessage response = null;

            int MaxRetryCount = 3;
            for (int i = 0; i <= MaxRetryCount; i++)
            {
                response = await this.client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                if (i == MaxRetryCount)
                {
                    continue;
                }

                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.TooManyRequests:
                        await Task.Delay(500);
                        break;
                    case System.Net.HttpStatusCode.GatewayTimeout:
                        await Task.Delay(200);
                        break;
                }
            }

            response?.EnsureSuccessStatusCode();
            return null;
        }
    }
}
