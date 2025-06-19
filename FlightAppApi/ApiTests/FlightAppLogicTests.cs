namespace Tests
{
    using System.Net;
    using FlightAppApi.Interfaces;
    using FlightAppApi.Services;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Caching.Memory;
    using Moq;
    using Moq.Protected;

    public class FlightAppLogicTests
    {
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<IMemoryCache> _mockCache;

        private const string Token = "test_token";
        private const string ApiUrl = "https://test-api.com/states";

        public FlightAppLogicTests() 
        {
            _mockTokenService = new Mock<ITokenService>();
            _mockConfig = new Mock<IConfiguration>();
            _mockCache = new Mock<IMemoryCache>();
        }

        [Fact]
        public async Task GetFlightLocation_FlightExists_ReturnsLocation()
        {
            // Arrange
            string flightNumber = "12345";
            var json = @"{ ""states"": [ [null, ""12345"", ""USA"", null, null, -80.123, 25.456] ] }";

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };

            var service = CreateService(response);

            // Act
            var result = await service.GetFlightLocation(flightNumber);

            // Assert
            result.Should().NotBeNull();
            result!.CallSign.Should().Be("12345");
            result.Latitude.Should().Be(25.456);
            result.Longitude.Should().Be(-80.123);
            result.OriginCountry.Should().Be("USA");

            _mockTokenService.Verify(o => o.GetTokenAsync(), Times.Once);
            _mockCache.Verify(o => o.Remove(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetFlightLocationl_NoToken_ReturnsNull()
        {
            // Arrange
            _mockTokenService.Setup(o => o.GetTokenAsync())
                             .ReturnsAsync((string?)null);

            var service = new FlightAppLogic(new HttpClient(), _mockTokenService.Object, _mockConfig.Object, _mockCache.Object);

            // Act
            var result = await service.GetFlightLocation("12345");

            // Assert
            result.Should().BeNull();
            _mockTokenService.Verify(o => o.GetTokenAsync(), Times.Once);
            _mockCache.Verify(o => o.Remove(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetFlightLocation_UnathorizedAccess_RemovesTokenFromCache()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            var service = CreateService(response);

            // Act
            var result = await service.GetFlightLocation("12345");

            // Assert
            result.Should().BeNull();
            _mockTokenService.Verify(o => o.GetTokenAsync(), Times.Once);;
            _mockCache.Verify(o => o.Remove(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetFlightLocation_FlightIsNotFound_ReturnsNull()
        {
            // Arrange
            string flightNumber = "12345";
            var json = @"{ ""states"": [ [null, ""NOTFOUND"", ""USA"", null, null, 10.1, 20.2] ] }";

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };

            var service = CreateService(response);

            // Act
            var result = await service.GetFlightLocation(flightNumber);

            // Assert
            result.Should().BeNull();
            _mockTokenService.Verify(o => o.GetTokenAsync(), Times.Once);
            _mockCache.Verify(o => o.Remove(It.IsAny<string>()), Times.Never);
        }

        private FlightAppLogic CreateService(HttpResponseMessage mockResponse)
        {
            // Setup fake HTTP handler
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(mockResponse);

            var httpClient = new HttpClient(handlerMock.Object);

            _mockTokenService.Setup(o => o.GetTokenAsync())
                             .ReturnsAsync(Token);

            _mockConfig.Setup(o => o["OpenSkyApi:StatesUrl"])
                       .Returns(ApiUrl);

            _mockCache.Setup(o => o.Remove(It.IsAny<string>()));

            return new FlightAppLogic(httpClient, _mockTokenService.Object, _mockConfig.Object, _mockCache.Object);
        }
    }
}
