using Newtonsoft.Json.Linq;
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
    internal class RequestManager
    {
        /// <summary>
        /// The HttpClient to use when issuing requests.
        /// </summary>
        private readonly HttpClient client = new HttpClient();

        /// <summary>
        /// The base api endpoint for PlayCEA.
        /// </summary>
        private const string apiEndpoint = "https://1ebv8yx4pa.execute-api.us-east-1.amazonaws.com";

        /// <summary>
        /// Creates a new request manager.
        /// </summary>
        internal RequestManager()
        {
            this.client.DefaultRequestHeaders.Accept.Clear();
            this.client.DefaultRequestHeaders.Add("User-Agent", "Play CEA Stats Collector");
        }

        /// <summary>
        /// Loads all brackets referenced by a collection of tournaments.
        /// </summary>
        /// <param name="tournaments">Collection of tournaments to populate data for.</param>
        /// <returns>A task to load all of the data.</returns>
        internal async Task LoadBrackets(List<Tournament> tournaments)
        {
            List<Task> allTasks = new List<Task>();
            foreach(Tournament t in tournaments)
            {
                foreach(Bracket b in t.Brackets)
                {
                    allTasks.Add(this.GetBracket(b.BracketId));
                }
            }

            await Task.WhenAll(allTasks);
        }

        /// <summary>
        /// Gets a bracket from PlayCEA.
        /// </summary>
        /// <param name="bracketId">The bracket id to read.</param>
        /// <returns>The fully populated Bracket.</returns>
        internal async Task<Bracket> GetBracket(string bracketId)
        {
            string content = await this.client.GetStringAsync($"{apiEndpoint}/prod/brackets/{bracketId}");
            JObject jObject = JObject.Parse(content);
            Bracket bracket = Marshaller.Bracket(jObject["data"][0]);
            return bracket;
        }

        /// <summary>
        /// Gets a specific match result from PlayCEA.
        /// </summary>
        /// <param name="matchId">The match id.</param>
        /// <returns>The most recent info for the match.</returns>
        internal async Task<MatchResult> GetMatchResult(string matchId)
        {
            string content = await this.client.GetStringAsync($"{apiEndpoint}/prod/matches/{matchId}");
            JObject jObject = JObject.Parse(content);
            MatchResult match = Marshaller.Match(jObject["data"]);
            return match;
        }

        /// <summary>
        /// Gets a specific team from PlayCEA.
        /// </summary>
        /// <param name="teamId">The team id</param>
        /// <returns>The updated team.</returns>
        internal async Task<Team> GetTeam(string teamId)
        {
            string content = await this.client.GetStringAsync($"{apiEndpoint}/prod/teams/{teamId}");
            JObject jObject = JObject.Parse(content);
            Team team = Marshaller.Team(jObject["data"][0]);
            return team;
        }

        /// <summary>
        /// Gets all tournaments from PlayCEA.
        /// </summary>
        /// <returns>Collection of all Tournaments from PlayCEA.</returns>
        internal async Task<List<Tournament>> GetTournaments() {
            string content = await this.client.GetStringAsync($"{apiEndpoint}/prod/tournaments");
            JObject jObject = JObject.Parse(content);
            List<Tournament> tournaments = new List<Tournament>();
            foreach (JToken t in jObject["data"]) {
                Tournament tournament = Marshaller.Tournament(t);
                tournaments.Add(tournament);
            }
            return tournaments;
        }

        /// <summary>
        /// Gets a specific tournament from PlayCEA.
        /// </summary>
        /// <param name="tournamentId">The tournament id.</param>
        /// <returns>The updated tournament.</returns>
        internal async Task<Tournament> GetTournament(string tournamentId) {
            string content = await this.client.GetStringAsync($"{apiEndpoint}/prod/tournaments/{tournamentId}");
            JObject jObject = JObject.Parse(content);
            Tournament tournament = Marshaller.Tournament(jObject["data"]);
            return tournament;
        }

        /// <summary>
        /// Updates all teams from a collection of teams.
        /// </summary>
        /// <param name="teams">The list of teams to update.</param>
        /// <returns>Task for updating the teams in parallel.</returns>
        internal async Task UpdateAllTeams(List<Team> teams)
        {
            List<Task> allTasks = new List<Task>();
            foreach (Team team in teams)
            {
                allTasks.Add(this.UpdateTeamDetails(team));
            }

            await Task.WhenAll(allTasks);
        }

        /// <summary>
        /// Updates the details for a single team.
        /// </summary>
        /// <param name="team">The team to update.</param>
        /// <returns>Task for updating the team.</returns>
        internal async Task UpdateTeamDetails(Team team)
        {
            await this.GetTeam(team.TeamId);
        }
    }
}
