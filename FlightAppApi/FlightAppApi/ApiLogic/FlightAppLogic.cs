namespace FlightAppApi.Services
{
    using FlightAppApi.Interfaces;
    using FlightAppApi.Models;
    using Microsoft.Extensions.Caching.Memory;
    using Newtonsoft.Json.Linq;
    using System.Net;
    using System.Net.Http.Headers;

    /// <summary>
    /// Flight application logic.
    /// </summary>
    public class FlightAppLogic : IFlightAppLogic
    {
        /// <summary>
        /// HTTP client instance.
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Token service instance.
        /// </summary>
        private readonly ITokenService _tokenService;
        
        /// <summary>
        /// Memory cahce instance.
        /// </summary>
        private readonly IMemoryCache _cache;

        /// <summary>
        /// Configuration instance.
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// Flight application logic.
        /// </summary>
        /// <param name="httpClient"></param>
        public FlightAppLogic(HttpClient httpClient, ITokenService tokenService, IConfiguration config, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _config = config;
            _cache = cache;
        }

        /// <summary>
        /// Gets flight location based on flight number.
        /// </summary>
        /// <param name="flightNumber">Flight number.</param>
        public async Task<FlightLocation?> GetFlightLocation(string flightNumber)
        {
            string? token = await _tokenService.GetTokenAsync();

            if (token == null) 
            {
                return null;
            }

            var req = new HttpRequestMessage(HttpMethod.Get, _config["OpenSkyApi:StatesUrl"]!);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(req);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _cache.Remove(TokenService.CacheKey);
                }
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();

            var data = JObject.Parse(json);
            var states = data["states"];

            if (states != null)
            {
                foreach (var state in states)
                {
                    if (string.Equals(state[1]?.ToString()?.Trim(), flightNumber, StringComparison.OrdinalIgnoreCase))
                    {
                        return new FlightLocation
                        {
                            CallSign = state[1]?.ToString()?.Trim() ?? string.Empty,
                            Latitude = state[6]?.ToObject<double>() ?? 0,
                            Longitude = state[5]?.ToObject<double>() ?? 0,
                            OriginCountry = state[2]?.ToString() ?? string.Empty,
                        };
                    }
                }
            }

            return null;
        }
    }
}
