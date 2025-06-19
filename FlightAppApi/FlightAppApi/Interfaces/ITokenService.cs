namespace FlightAppApi.Interfaces
{
    public interface ITokenService
    {
        /// <summary>
        /// Gets access token for OpenSky API.
        /// </summary>
        public Task<string?> GetTokenAsync();
    }
}
