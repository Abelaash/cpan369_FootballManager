using FootballManager.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        // ✅ GET TEAMS by League + Season
        public async Task<List<ApiTeamModel>> GetTeamsByLeagueAsync(int leagueId, int season)
        {
            using (var client = CreateClient())
            {
                string url = $"{baseUrl}/teams?league={leagueId}&season={season}";
                var response = await client.GetStringAsync(url);
                var result = JsonConvert.DeserializeObject<ApiFootballResponse>(response);

                return result.response.Select(item => new ApiTeamModel
                {
                    id = item.team.id,
                    name = item.team.name,
                    code = item.team.code,
                    country = item.team.country,
                    logo = item.team.logo,
                    stadiumName = item.venue?.name
                }).ToList();
            }
        }

        // ✅ GET PLAYERS by Team
        public async Task<List<Player>> GetPlayersByTeamAsync(int teamId, int season)
        {
            var allPlayers = new List<Player>();
            int page = 1;

            while (true)
            {
                string url = $"{baseUrl}/players?team={teamId}&season={season}&page={page}";

                using (var client = CreateClient())
                {
                    var response = await client.GetStringAsync(url);
                    var result = JsonConvert.DeserializeObject<ApiPlayerResponse>(response);

                    if (result?.response == null || result.response.Count == 0)
                        break;

                    foreach (var wrapper in result.response)
                    {
                        var playerData = wrapper.player;
                        var stat = wrapper.statistics?.FirstOrDefault();

                        allPlayers.Add(new Player
                        {
                            FirstName = playerData.firstname,
                            LastName = playerData.lastname,
                            Age = playerData.age,
                            Nationality = playerData.nationality,
                            Height = ConvertToDecimal(playerData.height),
                            Weight = ConvertToDecimal(playerData.weight),
                            Position = stat?.games?.position ?? playerData.position,
                            TeamName = stat?.team?.name,
                            TeamId = stat?.team?.id ?? 0,
                            LeagueName = stat?.league?.name,
                            TotalGoals = stat?.goals?.total ?? 0,
                            TotalShots = stat?.shots?.total ?? 0,
                            TotalPasses = stat?.passes?.total ?? 0,
                            Assists = stat?.goals?.assists ?? 0,
                            Appearances = stat?.games?.appearences ?? 0,
                            PhotoUrl = playerData.photo
                        });
                    }

                    if (result.paging == null || result.paging.current >= result.paging.total)
                        break;

                    page++;
                    await Task.Delay(1000); // Respect API rate limit
                }
            }

            return allPlayers;
        }

        private decimal ConvertToDecimal(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return 0;
            var numberPart = new string(input.Where(char.IsDigit).ToArray());
            return decimal.TryParse(numberPart, out decimal result) ? result : 0;
        }

        // ✅ LEAGUE STANDINGS
        public List<Team> GetLeagueStandings(string leagueName)
        {
            var leagueIds = new Dictionary<string, int>
            {
                { "Premier League", 39 }, { "La Liga", 140 }, { "Bundesliga", 78 },
                { "Ligue 1", 61 }, { "Serie A", 135 }, { "Eredivisie", 88 }
            };

            if (!leagueIds.TryGetValue(leagueName, out int leagueId))
                return new List<Team>();

            string url = $"{baseUrl}/standings?season=2024&league={leagueId}";

            using (var client = CreateClient())
            {
                var response = client.GetAsync(url).Result;
                if (!response.IsSuccessStatusCode) return new List<Team>();

                var json = response.Content.ReadAsStringAsync().Result;
                var data = JsonConvert.DeserializeObject<StandingsApiResponse>(json);

                return data.response[0].league.standings[0].Select(team => new Team
                {
                    TeamId = team.team.id,
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
                }).ToList();
            }
        }

        // ✅ UPCOMING MATCHES
        public List<Match> GetUpcomingMatches(string leagueName)
        {
            var leagueMap = new Dictionary<string, int>
            {
                { "Premier League", 39 }, { "La Liga", 140 }, { "Serie A", 135 },
                { "Bundesliga", 78 }, { "Ligue 1", 61 }, { "Eredivisie", 88 }
            };

            if (!leagueMap.TryGetValue(leagueName, out int leagueId))
                return new List<Match>();

            string url = $"{baseUrl}/fixtures?league={leagueId}&season=2024&next=10";

            using (var client = CreateClient())
            {
                var response = client.GetStringAsync(url).Result;
                dynamic json = JsonConvert.DeserializeObject(response);

                return ((IEnumerable<dynamic>)json.response).Select(fixture => new Match
                {
                    MatchDate = DateTime.Parse((string)fixture.fixture.date),
                    Venue = fixture.fixture.venue.name ?? "Unknown",
                    Status = "Upcoming",
                    HomeTeamName = fixture.teams.home.name,
                    AwayTeamName = fixture.teams.away.name,
                    HomeTeamLogo = fixture.teams.home.logo,
                    AwayTeamLogo = fixture.teams.away.logo
                }).ToList();
            }
        }

        // Fixtures by team
        public List<Match> GetUpcomingMatchesByTeam(int teamId, int count = 3)
        {
            string url = $"{baseUrl}/fixtures?season=2024&team={teamId}&next={count}";

            using (var client = CreateClient())
            {
                var response = client.GetStringAsync(url).Result;
                dynamic json = JsonConvert.DeserializeObject(response);

                return ((IEnumerable<dynamic>)json.response).Select(fixture => new Match
                {
                    MatchDate = DateTime.Parse((string)fixture.fixture.date),
                    Venue = fixture.fixture.venue.name ?? "Unknown",
                    Status = "Upcoming",
                    HomeTeamName = fixture.teams.home.name,
                    AwayTeamName = fixture.teams.away.name,
                    HomeTeamLogo = fixture.teams.home.logo,
                    AwayTeamLogo = fixture.teams.away.logo
                }).ToList();
            }
        }

        // ✅ INJURIES
        public async Task<List<Injury>> GetInjuriesByLeagueAndSeason(int leagueId, int season)
        {
            using (var context = new TeamContext())
            using (var client = CreateClient())
            {
                var response = await client.GetAsync($"{baseUrl}/injuries?league={leagueId}&season={season}");

                if (!response.IsSuccessStatusCode)
                    return new List<Injury>();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiInjuryResponse>(json);

                return result.Response.Select(i =>
                {
                    var player = context.Players.FirstOrDefault(p =>
                        (p.FirstName + " " + p.LastName).Trim().ToLower() == i.Player.Name.Trim().ToLower());

                    var team = context.Teams.FirstOrDefault(t =>
                        t.Name.Trim().ToLower() == i.Team.Name.Trim().ToLower());

                    return new Injury
                    {
                        PlayerId = player?.PlayerId ?? 0,
                        TeamId = team?.TeamId ?? 0,
                        InjuryType = i.Type ?? "Unknown",
                        Severity = i.Reason ?? "Moderate",
                        DateInjured = i.Start,
                        ExpectedRecoveryDate = i.End ?? i.Start.AddDays(14),
                        Notes = i.Reason
                    };
                }).ToList();
            }
        }

        // ✅ COACHES
        public async Task<List<ApiCoachDto>> GetCoachesByLeagueAsync(string leagueName)
        {
            var leagueIds = new Dictionary<string, int>
            {
                { "Premier League", 39 }, { "La Liga", 140 }, { "Bundesliga", 78 },
                { "Ligue 1", 61 }, { "Serie A", 135 }, { "Eredivisie", 88 }
            };

            if (!leagueIds.TryGetValue(leagueName, out int leagueId))
                return new List<ApiCoachDto>();

            int season = 2024;
            var teams = await GetTeamsByLeagueAsync(leagueId, season);
            var coaches = new List<ApiCoachDto>();

            using (var client = CreateClient())
            {
                foreach (var team in teams)
                {
                    var url = $"{baseUrl}/coachs?team={team.id}";
                    var response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode) continue;

                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiCoachResponse>(json);

                    if (result?.response?.Any() == true)
                    {
                        var coach = result.response.First();
                        coaches.Add(new ApiCoachDto
                        {
                            FirstName = coach.firstname,
                            LastName = coach.lastname,
                            Team = team.name
                        });
                    }
                }
            }

            return coaches;
        }

        public int GetApiTeamId(string teamName)
        {
            if (TeamNameToApiId.TryGetValue(teamName, out int apiId))
                return apiId;

            throw new Exception($"API ID for team '{teamName}' not found. Please update TeamNameToApiId mapping.");
        }

        private static readonly Dictionary<string, int> TeamNameToApiId = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            // Premier League Teams
            { "Manchester United", 33 },
            { "Newcastle", 34 },
            { "Bournemouth", 35 },
            { "Fulham", 36 },
            { "Wolves", 39 },
            { "Liverpool", 40 },
            { "Southampton", 41 },
            { "Arsenal", 42 },
            { "Everton", 45 },
            { "Leicester", 46 },
            { "Tottenham", 47 },
            { "West Ham", 48 },
            { "Chelsea", 49 },
            { "Manchester City", 50 },
            { "Brighton", 51 },
            { "Crystal Palace", 52 },
            { "Brentford", 55 },
            { "Ipswich", 57 },
            { "Nottingham Forest", 65 },
            { "Aston Villa", 66 },

            // La Liga Teams
            { "Barcelona", 529 },
            { "Atletico Madrid", 530 },
            { "Athletic Club", 531 },
            { "Valencia", 532 },
            { "Villarreal", 533 },
            { "Las Palmas", 534 },
            { "Sevilla", 536 },
            { "Leganes", 537 },
            { "Celta Vigo", 538 },
            { "Espanyol", 540 },
            { "Real Madrid", 541 },
            { "Alaves", 542 },
            { "Real Betis", 543 },
            { "Getafe", 546 },
            { "Girona", 547 },
            { "Real Sociedad", 548 },
            { "Valladolid", 720 },
            { "Osasuna", 727 },
            { "Rayo Vallecano", 728 },
            { "Mallorca", 798 },

            // Bundesliga Teams (2024)
            { "Bayern München", 157 },
            { "SC Freiburg", 160 },
            { "VfL Wolfsburg", 161 },
            { "Werder Bremen", 162 },
            { "Borussia Mönchengladbach", 163 },
            { "FSV Mainz 05", 164 },
            { "Borussia Dortmund", 165 },
            { "1899 Hoffenheim", 167 },
            { "Bayer Leverkusen", 168 },
            { "Eintracht Frankfurt", 169 },
            { "FC Augsburg", 170 },
            { "VfB Stuttgart", 172 },
            { "RB Leipzig", 173 },
            { "VfL Bochum", 176 },
            { "1. FC Heidenheim", 180 },
            { "Union Berlin", 182 },
            { "FC St. Pauli", 186 },
            { "Holstein Kiel", 191 },

            // Ligue 1 Teams
            { "Angers", 77 },
            { "Lille", 79 },
            { "Lyon", 80 },
            { "Marseille", 81 },
            { "Montpellier", 82 },
            { "Nantes", 83 },
            { "Nice", 84 },
            { "Paris Saint Germain", 85 },
            { "Monaco", 91 },
            { "Reims", 93 },
            { "Rennes", 94 },
            { "Strasbourg", 95 },
            { "Toulouse", 96 },
            { "Stade Brestois 29", 106 },
            { "Auxerre", 108 },
            { "LE Havre", 111 },
            { "Lens", 116 },
            { "Saint Etienne", 1063 },

            // Serie A Teams
            { "Lazio", 487 },
            { "AC Milan", 489 },
            { "Cagliari", 490 },
            { "Napoli", 492 },
            { "Udinese", 494 },
            { "Genoa", 495 },
            { "Juventus", 496 },
            { "AS Roma", 497 },
            { "Atalanta", 499 },
            { "Bologna", 500 },
            { "Fiorentina", 502 },
            { "Torino", 503 },
            { "Verona", 504 },
            { "Inter", 505 },
            { "Empoli", 511 },
            { "Venezia", 517 },
            { "Parma", 523 },
            { "Lecce", 867 },
            { "Como", 895 },
            { "Monza", 1579 },

            // Eredivisie Teams
            { "PEC Zwolle", 193 },
            { "Ajax", 194 },
            { "Willem II", 195 },
            { "PSV Eindhoven", 197 },
            { "AZ Alkmaar", 201 },
            { "Groningen", 202 },
            { "NAC Breda", 203 },
            { "Fortuna Sittard", 205 },
            { "Heracles", 206 },
            { "Utrecht", 207 },
            { "Feyenoord", 209 },
            { "Heerenveen", 210 },
            { "GO Ahead Eagles", 410 },
            { "NEC Nijmegen", 413 },
            { "Twente", 415 },
            { "Waalwijk", 417 },
            { "Almere City FC", 419 },
            { "Sparta Rotterdam", 426 }
        };
    }
}
