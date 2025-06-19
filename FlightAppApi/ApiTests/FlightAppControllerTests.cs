namespace Tests
{
    using Xunit;
    using Moq;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using FlightAppApi.Controllers;
    using FlightAppApi.Interfaces;
    using FlightAppApi.Models;

    public class FlightControllerTests
    {
        private readonly Mock<IFlightAppLogic> _mockFlightLogic;
        private readonly FlightController _controller;

        public FlightControllerTests()
        {
            _mockFlightLogic = new Mock<IFlightAppLogic>();
            _controller = new FlightController(_mockFlightLogic.Object);
        }

        [Fact]
        public async Task GetFlightLocation_FlightIsFound_ReturnsOk()
        {
            // Arrange
            var flightNumber = "12345";
            var expectedLocation = new FlightLocation
            {
                CallSign = flightNumber,
                Latitude = 40.0,
                Longitude = -74.0,
                OriginCountry = "USA"
            };

            _mockFlightLogic
                .Setup(o => o.GetFlightLocation(flightNumber))
                .ReturnsAsync(expectedLocation);

            // Act
            var result = await _controller.GetFlightLocation(flightNumber);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(expectedLocation);
        }

        [Fact]
        public async Task GetFlightLocation_FlightIsNotFound_ReturnsNotFound()
        {
            // Arrange
            var flightNumber = "12345";
            _mockFlightLogic
                .Setup(o => o.GetFlightLocation(flightNumber))
                .ReturnsAsync((FlightLocation?)null);

            // Act
            var result = await _controller.GetFlightLocation(flightNumber);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be(404);
            notFoundResult.Value.Should().Be("Flight not found or failed to fetch data.");
        }
    }
}
