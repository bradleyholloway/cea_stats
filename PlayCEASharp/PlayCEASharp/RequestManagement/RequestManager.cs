using PlayCEASharp.Configuration;
using PlayCEASharp.DataModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
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
        public RequestManager(string optionalEndpoint)
        {
            this.endpoint = optionalEndpoint ?? apiEndpoint;
            this.client.DefaultRequestHeaders.Accept.Clear();
            this.client.DefaultRequestHeaders.Add("User-Agent", "Play CEA Stats Collector");
        }

        /// <summary>
        /// Loads all brackets referenced by a collection of tournaments.
        /// </summary>
        /// <param name="tournaments">Collection of tournaments to populate data for.</param>
        /// <param name="tc">The TournamentConfiguration used for these brackets.</param>
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
        /// <param name="tc">The TournamentConfiguration for this bracket.</param>
        /// <returns>The fully populated Bracket.</returns>
        internal async Task<Bracket> GetBracket(string bracketId, TournamentConfiguration tc)
        {
            string content = await this.GetStringWithRetryAsync($"{endpoint}/brackets/{bracketId}");
            JsonObject jObject = JsonNode.Parse(content).AsObject();
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
            JsonObject jObject = JsonNode.Parse(content).AsObject();
            MatchResult match = Marshaller.Match(jObject["data"]);
            return match;
        }

        /// <summary>
        /// Gets a specific team from PlayCEA.
        /// </summary>
        /// <param name="teamId">The team id</param>
        /// <param name="tc">The TournamentConfiguration for this team.</param>
        /// <returns>The updated team.</returns>
        public async Task<Team> GetTeam(string teamId, TournamentConfiguration tc)
        {
            string content = await this.GetStringWithRetryAsync($"{endpoint}/teams/{teamId}");
            JsonObject jObject = JsonNode.Parse(content).AsObject();
            Team team = Marshaller.Team(jObject["data"][0], tc);
            return team;
        }

        /// <summary>
        /// Reports scores for a match using a given discord bearer token.
        /// </summary>
        /// <param name="match">The match to report scores for.</param>
        /// <param name="discordBearerToken">Bearer token to use to update.</param>
        public async Task<HttpResponseMessage> ReportScores(MatchResult match, string discordBearerToken)
        {
            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}/matches/{match.MatchId}/scores");
            httpRequest.Headers.Add("authorization", $"bearer {discordBearerToken}");
            StringContent content = new StringContent(MatchResultToScoresUpdate(match));
            httpRequest.Content = content;
            return await this.client.SendAsync(httpRequest);

        }

        /// <summary>
        /// Reports scores for a match using a given discord bearer token.
        /// </summary>
        /// <param name="teamName">The name of the team to create.</param>
        /// <param name="teamOrg">The org of the team to create.</param>
        /// <param name="teamCharity">The charity for the team to create.</param>
        /// <param name="discordBearerToken">Bearer token to use to update.</param>
        /// <returns>The created team, or null if failed.</returns>
        public async Task<Team> CreateTeam(string teamName, string teamOrg, string teamCharity, string discordBearerToken)
        {
            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}/teams");
            httpRequest.Headers.Add("authorization", $"bearer {discordBearerToken}");
            StringContent content = new StringContent(FormatNewTeamJson(teamName, teamOrg, teamCharity));
            httpRequest.Content = content;
            var response = await this.client.SendAsync(httpRequest);

            string responseContent = await response.Content.ReadAsStringAsync();
            JsonObject jObject = JsonNode.Parse(responseContent).AsObject();
            Team team = Marshaller.Team(jObject["data"]);

            return team;
        }

        /// <summary>
        /// Reports scores for a match using a given discord bearer token.
        /// </summary>
        /// <param name="teamId">The teamId to add.</param>
        /// <param name="tournamentId">The tournamentId to add.</param>
        /// <param name="discordBearerToken">Bearer token to use to update.</param>
        /// <returns>The created team, or null if failed.</returns>
        public async Task<string> AddTeamToTournament(string teamId, string tournamentId, string discordBearerToken)
        {
            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Put, $"{endpoint}/tournament/{tournamentId}/teams");
            httpRequest.Headers.Add("authorization", $"bearer {discordBearerToken}");
            StringContent content = new StringContent(FormatAddTeamToTournamentJson(teamId));
            httpRequest.Content = content;
            var response = await this.client.SendAsync(httpRequest);

            string responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }

        /// <summary>
        /// Generates a new Invite Code for a team.
        /// </summary>
        /// <param name="teamId">The teamId to add.</param>
        /// <param name="discordBearerToken">Bearer token to use to update.</param>
        /// <returns>The created team, or null if failed.</returns>
        public async Task<string> GenerateNewInviteCode(string teamId, string discordBearerToken)
        {
            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}/teams/{teamId}/invite");
            httpRequest.Headers.Add("authorization", $"bearer {discordBearerToken}");
            var response = await this.client.SendAsync(httpRequest);
            
            string responseContent = await response.Content.ReadAsStringAsync();
            JsonObject jObject = JsonNode.Parse(responseContent).AsObject();
            string newCode = jObject["data"].GetValue<string>();
            return newCode;
        }

        /// <summary>
        /// Formats a new team request json.
        /// </summary>
        /// <param name="teamId">The id of the team to create.</param>
        /// <returns>Json formatted new team request.</returns>
        private string FormatAddTeamToTournamentJson(string teamId)
        {
            return $"{{\"teams\":[{{\"tid\":\"{teamId}\"}}]}}";
        }

        /// <summary>
        /// Formats a new team request json.
        /// </summary>
        /// <param name="teamName">The name of the team to create.</param>
        /// <param name="teamOrg">The org of the team to create.</param>
        /// <param name="teamCharity">The charity for the team to create.</param>
        /// <returns>Json formatted new team request.</returns>
        private string FormatNewTeamJson(string teamName, string teamOrg, string teamCharity)
        {
            return $"{{\"name\":\"{teamName}\",\"org\":\"{teamOrg}\",\"charity\":\"{teamCharity}\"}}";
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
        internal async Task<List<Tournament>> GetTournaments() {
            string content = await this.GetStringWithRetryAsync($"{endpoint}/tournaments");
            JsonObject jObject = JsonNode.Parse(content).AsObject();
            List<Tournament> tournaments = new List<Tournament>();
            foreach (JsonNode t in jObject["data"].AsArray()) {
                Tournament tournament = Marshaller.Tournament(t);
                tournaments.Add(tournament);
            }
            return tournaments;
        }

        /// <summary>
        /// Gets a specific tournament from PlayCEA.
        /// </summary>
        /// <param name="tournamentId">The tournament id.</param>
        /// <param name="tc">The TournamentConfiguration for this tournament.</param>
        /// <returns>The updated tournament.</returns>
        internal async Task<Tournament> GetTournament(string tournamentId, TournamentConfiguration tc)
        {
            string content = await this.GetStringWithRetryAsync($"{endpoint}/tournaments/{tournamentId}");
            JsonObject jObject = JsonNode.Parse(content).AsObject();
            Tournament tournament = Marshaller.Tournament(jObject["data"]);
            return tournament;
        }

        /// <summary>
        /// Updates all teams from a collection of teams.
        /// </summary>
        /// <param name="teams">The list of teams to update.</param>
        /// <param name="tc">The TournamentConfiguration for these teams.</param>
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
        /// <param name="tc">The TournamentConfiguration for the team.</param>
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
