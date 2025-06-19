namespace FlightAppApi.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using FlightAppApi.Interfaces;

    /// <summary>
    /// Flight application controller.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FlightController : ControllerBase
    {
        /// <summary>
        /// Flight logic instance.
        /// </summary>
        private readonly IFlightAppLogic _flightLogic;

        /// <summary>
        /// Flight application controller.
        /// </summary>
        /// <param name="flightLogic"></param>
        public FlightController(IFlightAppLogic flightLogic)
        {
            _flightLogic = flightLogic;
        }

        /// <summary>
        /// Gets flight location.
        /// </summary>
        /// <param name="flightNumber">Flight numbewr.</param>
        [HttpGet("{flightNumber}")]
        public async Task<IActionResult> GetFlightLocation(string flightNumber)
        {
            var result = await _flightLogic.GetFlightLocation(flightNumber);

            if (result == null) 
            {
                return NotFound("Flight not found or failed to fetch data.");
            }

            return Ok(result);
        }
    }
}
