using FootballManager.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace FootballManager.API
{
    public class ApiFootballService
    {
        private readonly string apiKey = "8c4793e6f4msh348039bcb250610p18043djsnf2458de97956";
        private readonly string baseUrl = "https://api-football-v1.p.rapidapi.com/v3";

        private HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-RapidAPI-Key", apiKey);
            client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "api-football-v1.p.rapidapi.com");
            return client;
        }

        public async Task<List<ApiTeamModel>> GetTeamsByLeagueAsync(int leagueId, int season)
        {
            using (var client = CreateClient())
            {
                string url = $"{baseUrl}/teams?league={leagueId}&season={season}";
                var response = await client.GetStringAsync(url);
                var result = JsonConvert.DeserializeObject<ApiFootballResponse>(response);
                var teams = new List<ApiTeamModel>();

                foreach (var item in result.response)
                {
                    teams.Add(item.team);
                }

                return teams;
            }
        }

        public async Task<List<ApiPlayerWrapper>> GetPlayersByTeamAsync(int teamId, int season)
        {
            using (var client = CreateClient())
            {
                string url = $"{baseUrl}/players?team={teamId}&season={season}";
                var response = await client.GetStringAsync(url);
                var result = JsonConvert.DeserializeObject<ApiPlayerResponse>(response);
                return result.response;
            }
        }

        public List<Team> GetLeagueStandings(string leagueName)
        {
            var leagueIds = new Dictionary<string, int>
            {
                { "Premier League", 39 },
                { "La Liga", 140 },
                { "Bundesliga", 78 },
                { "Ligue 1", 61 },
                { "Serie A", 135 },
                { "Eredivisie", 88 }
            };

            if (!leagueIds.ContainsKey(leagueName))
                return new List<Team>();

            int leagueId = leagueIds[leagueName];
            int season = 2024;

            using (var client = CreateClient())
            {
                var url = $"{baseUrl}/standings?season={season}&league={leagueId}";
                var response = client.GetAsync(url).Result;

                if (!response.IsSuccessStatusCode)
                    return new List<Team>();

                var json = response.Content.ReadAsStringAsync().Result;
                var data = JsonConvert.DeserializeObject<StandingsApiResponse>(json);

                var standings = new List<Team>();

                foreach (var team in data.response[0].league.standings[0])
                {
                    standings.Add(new Team
                    {
                        Name = team.team.name,
                        LogoUrl = team.team.logo, 
                        Wins = team.all.win,
                        Losses = team.all.lose,
                        Draws = team.all.draw,
                        matches_played = team.all.played,
                        goals_for = team.all.goals.@for,
                        goals_against = team.all.goals.against,
                        Points = team.points,
                        League = leagueName
                    });

                }

                return standings;
            }
        }

        public List<Match> GetUpcomingMatches(string leagueName)
        {
            var leagueMap = new Dictionary<string, int>
            {
                { "Premier League", 39 },
                { "La Liga", 140 },
                { "Serie A", 135 },
                { "Bundesliga", 78 },
                { "Ligue 1", 61 },
                { "Eredivisie", 88 }
            };

            if (!leagueMap.ContainsKey(leagueName))
                return new List<Match>();

            int leagueId = leagueMap[leagueName];
            string url = $"{baseUrl}/fixtures?league={leagueId}&season=2024&next=10";

            using (var client = CreateClient())
            {
                var response = client.GetStringAsync(url).Result;
                dynamic json = JsonConvert.DeserializeObject(response);

                var matches = new List<Match>();

                foreach (var fixture in json.response)
                {
                    matches.Add(new Match
                    {
                        MatchDate = DateTime.Parse((string)fixture.fixture.date),
                        Venue = fixture.fixture.venue.name ?? "Unknown",
                        Status = "Upcoming",
                        HomeTeamName = fixture.teams.home.name,
                        AwayTeamName = fixture.teams.away.name,
                        HomeTeamLogo = fixture.teams.home.logo,
                        AwayTeamLogo = fixture.teams.away.logo
                    });
                }

                return matches;
            }
        }
    }
}
