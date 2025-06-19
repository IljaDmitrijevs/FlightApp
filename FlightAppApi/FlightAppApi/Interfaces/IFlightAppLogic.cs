namespace FlightAppApi.Interfaces
{
    using FlightAppApi.Models;

    /// <summary>
    /// Flight applicatioin logic interface.
    /// </summary>
    public interface IFlightAppLogic
    {
        /// <summary>
        /// Gets flight location based on flight number.
        /// </summary>
        /// <param name="flightNumber">Flight number.</param>
        public Task<FlightLocation?> GetFlightLocation(string flightNumber);
    }
}
