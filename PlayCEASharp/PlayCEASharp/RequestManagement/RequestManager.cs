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
    public class RequestManager
    {
        private readonly HttpClient client = new HttpClient();
        private const string apiEndpoint = "https://1ebv8yx4pa.execute-api.us-east-1.amazonaws.com";

        public RequestManager()
        {
            this.client.DefaultRequestHeaders.Accept.Clear();
            this.client.DefaultRequestHeaders.Add("User-Agent", "Play CEA Stats Collector");
        }

        public async Task LoadBrackets(List<Tournament> tournaments)
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

        public async Task<Bracket> GetBracket(string bracketId)
        {
            string content = await this.client.GetStringAsync($"{apiEndpoint}/prod/brackets/{bracketId}");
            JObject jObject = JObject.Parse(content);
            Bracket bracket = Marshaller.Bracket(jObject["data"][0]);
            return bracket;
        }

        public async Task<MatchResult> GetMatchResult(string matchId)
        {
            string content = await this.client.GetStringAsync($"{apiEndpoint}/prod/matches/{matchId}");
            JObject jObject = JObject.Parse(content);
            MatchResult match = Marshaller.Match(jObject["data"]);
            return match;
        }

        public async Task<Team> GetTeam(string teamId)
        {
            string content = await this.client.GetStringAsync($"{apiEndpoint}/prod/teams/{teamId}");
            JObject jObject = JObject.Parse(content);
            Team team = Marshaller.Team(jObject["data"][0]);
            return team;
        }

        public async Task<List<Tournament>> GetTournaments() {
            string content = await this.client.GetStringAsync($"{apiEndpoint}/prod/tournaments");
            JObject jObject = JObject.Parse(content);
            List<Tournament> tournaments = new List<Tournament>();
            foreach (JToken t in jObject["data"]) {
                Tournament tournament = Marshaller.Tournament(t);
                tournaments.Add(tournament);
            }
            return tournaments;
        }

        public async Task<Tournament> GetTournament(string tournamentId) {
            string content = await this.client.GetStringAsync($"{apiEndpoint}/prod/tournaments/{tournamentId}");
            JObject jObject = JObject.Parse(content);
            Tournament tournament = Marshaller.Tournament(jObject["data"]);
            return tournament;
        }
        
        public async Task UpdateAllTeams(List<Team> teams)
        {
            List<Task> allTasks = new List<Task>();
            foreach (Team team in teams)
            {
                allTasks.Add(this.UpdateTeamDetails(team));
            }

            await Task.WhenAll(allTasks);
        }

        public async Task UpdateTeamDetails(Team team)
        {
            await this.GetTeam(team.TeamId);
        }
    }
}
