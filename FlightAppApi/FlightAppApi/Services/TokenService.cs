namespace FlightAppApi.Services
{
    using FlightAppApi.Interfaces;
    using Microsoft.Extensions.Caching.Memory;
    using System.Text.Json;

    /// <summary>
    /// Token service for OpenSky API.
    /// </summary>
    public class TokenService : ITokenService
    {
        /// <summary>
        /// HTTP client instance.
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Configuration instance.
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// Memory cache instance.
        /// </summary>
        private readonly IMemoryCache _cache;

        /// <summary>
        /// Logger isntance.
        /// </summary>
        private readonly ILogger<TokenService> _logger;

        /// <summary>
        /// Cache key.
        /// </summary>
        public const string CacheKey = "OpenSkyAccessToken";

        /// <summary>
        /// Token service for OpenSky API.
        /// </summary>
        /// <param name="httpClient">HTTP client.</param>
        /// <param name="config">Configuration.</param>
        /// <param name="cache">Cache.</param>
        /// <param name="logger">Logger.</param>
        public TokenService(HttpClient httpClient, IConfiguration config, IMemoryCache cache, ILogger<TokenService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Gets access token for OpenSky API.
        /// </summary>
        public async Task<string?> GetTokenAsync()
        {
            if (_cache.TryGetValue(CacheKey, out string? token))
            {
                return token;
            }

            var tokenUrl = _config["OpenSkyAuth:TokenUrl"]!;
            var clientId = _config["OpenSkyAuth:ClientId"]!;
            var clientSecret = _config["OpenSkyAuth:ClientSecret"]!;

            var content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
            ]);

            var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
            {
                Content = content
            };

            request.Headers.Add("Accept", "application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed retrieving token: {Status}", response.StatusCode);

                return null;
            }

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var root = doc.RootElement;

            if (!root.TryGetProperty("access_token", out var t) || !root.TryGetProperty("expires_in", out var exp))
            {
                _logger.LogError("Invalid token response format");
                return null;
            }

            token = t.GetString()!;
            var expiry = exp.GetInt32();

            _cache.Set(CacheKey, token, TimeSpan.FromSeconds(expiry - 60));

            return token;
        }
    }
}
