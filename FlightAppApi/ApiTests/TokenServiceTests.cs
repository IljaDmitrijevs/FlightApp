namespace Tests
{
    using Moq;
    using Moq.Protected;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using System.Net;
    using FlightAppApi.Services;

    public class TokenServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly IMemoryCache _cache;
        private readonly Mock<ILogger<TokenService>> _mockLogger;

        public TokenServiceTests()
        {
            _mockConfig = new Mock<IConfiguration>();
            _cache = new MemoryCache(new MemoryCacheOptions());
            _mockLogger = new Mock<ILogger<TokenService>>();
        }

        [Fact]
        public async Task GetTokenAsync_TokenExists_ReturnsTokenFromCache()
        {
            // Arrange
            var expectedToken = "cached_token";
            _cache.Set(TokenService.CacheKey, expectedToken);

            var tokenService = new TokenService(new HttpClient(), _mockConfig.Object, _cache, _mockLogger.Object);

            // Act
            var result = await tokenService.GetTokenAsync();

            // Assert
            result.Should().Be(expectedToken);
        }

        [Fact]
        public async Task GetTokenAsync_TokenFetchedFromApi_ReturnsTokenAndCacheIt()
        {
            // Arrange
            var tokenResponse = "{\"access_token\":\"api_token_123\",\"expires_in\":3600}";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(tokenResponse)
            };

            var tokenService = CreateService(response);

            // Act
            var result = await tokenService.GetTokenAsync();

            // Assert
            result.Should().Be("api_token_123");

            _cache.TryGetValue(TokenService.CacheKey, out string? cachedToken);
            cachedToken.Should().Be("api_token_123");
        }

        [Fact]
        public async Task GetTokenAsync_ApiCallFails_ReturnsNull()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var tokenService = CreateService(response);

            // Act
            var result = await tokenService.GetTokenAsync();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetTokenAsync_MissingTokenInJson_ReturnsNull()
        {
            // Arrange
            var badJson = "{\"expires_in\":3600}";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(badJson)
            };

            var tokenService = CreateService(response);

            // Act
            var result = await tokenService.GetTokenAsync();

            // Assert
            result.Should().BeNull();
        }

        private TokenService CreateService(HttpResponseMessage responseMessage) 
        {
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            var httpClient = new HttpClient(handler.Object);

            _mockConfig.Setup(c => c["OpenSkyAuth:TokenUrl"]).Returns("https://fake-token-url");
            _mockConfig.Setup(c => c["OpenSkyAuth:ClientId"]).Returns("client_id");
            _mockConfig.Setup(c => c["OpenSkyAuth:ClientSecret"]).Returns("client_secret");

            return new TokenService(httpClient, _mockConfig.Object, _cache, _mockLogger.Object);
        }
    }
}
