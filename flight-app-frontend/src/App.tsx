import { useState, useEffect } from 'react';
import axios from 'axios';
import { MapContainer, TileLayer, Popup, Polyline } from 'react-leaflet';
import { FlightLocation } from './interfaces/FlightLocation';
import 'leaflet/dist/leaflet.css';
import L from 'leaflet';
import ReactLeafletDriftMarker from 'react-leaflet-drift-marker';
import './App.css';

// Airplane icon from public folder
const airplaneIcon = new L.Icon({
  iconUrl: '/icon/airplane.png',
  iconSize: [40, 40],
  iconAnchor: [20, 20],
  popupAnchor: [0, -20],
});

function App() {
  const [flightNumber, setFlightNumber] = useState('');
  const [location, setLocation] = useState<FlightLocation | null>(null);
  const [error, setError] = useState('');
  const [aircraftPath, setPath] = useState<[number, number][]>([]);
  const API_BASE_URL = process.env.REACT_APP_API_URL;

  useEffect(() => {
    if (!flightNumber) return;

    const fetchLocation = async () => {
      try {
        const response = await axios.get<FlightLocation>(
        `${API_BASE_URL}/api/flight/${flightNumber}`
      );
        const newLocation = response.data;
        setLocation(newLocation);

        // Add to path if valid coordinates
        if (newLocation.latitude && newLocation.longitude) {
          setPath((prevPath) => [...prevPath, [newLocation.latitude, newLocation.longitude]]);
        }

        setError('');
      } catch {
        setError('Flight not found or error fetching data.');
        setLocation(null);
      }
    };

    fetchLocation();

    // Set interval at that time data will be fetched. (every 10 seconds api limitations)
    const interval = setInterval(fetchLocation, 10000);

    return () => clearInterval(interval);
  }, [flightNumber]);

return (
  <div className="app-container">
    <h1>Flight Tracker</h1>
    <input
      placeholder="Enter flight number"
      value={flightNumber}
      onChange={(e) => setFlightNumber(e.target.value)}
      className="flight-input"
    />
    {error && <p className="error-text">{error}</p>}
    <div className="map-wrapper">
      <MapContainer
        center={[location?.latitude ?? 0, location?.longitude ?? 0]}
        zoom={location ? 6 : 2}
        className="leaflet-container"
      >
        <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />
        {aircraftPath.length > 1 && <Polyline positions={aircraftPath} color="blue" />}
        {location && (
          <ReactLeafletDriftMarker
            position={[location.latitude, location.longitude]}
            duration={4000}
            icon={airplaneIcon}
            keepAtCenter={false}
          >
            <Popup>
              <strong>{location.callSign}</strong> <br />
              {location.originCountry}
            </Popup>
          </ReactLeafletDriftMarker>
        )}
      </MapContainer>
    </div>
  </div>
);
}

export default App;
