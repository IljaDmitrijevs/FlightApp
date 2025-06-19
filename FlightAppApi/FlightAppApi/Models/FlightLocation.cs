namespace FlightAppApi.Models
{
    /// <summary>
    /// Flight location model.
    /// </summary>
    public class FlightLocation
    {
        /// <summary>
        /// Flight number.
        /// </summary>
        public string CallSign { get; set; } = string.Empty;

        /// <summary>
        /// Latitude.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude.
        /// </summary>
        public double Longitude { get; set; }
            
        /// <summary>
        /// Aircraft origin country.
        /// </summary>
        public string OriginCountry { get; set; } = string.Empty;
    }
}
