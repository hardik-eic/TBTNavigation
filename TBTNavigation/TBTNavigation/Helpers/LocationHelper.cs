using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;

namespace TBTNavigation.Helpers
{
    public static class LocationHelper
    {
        /// <summary>
        /// Retrieves the current location of the device.
        /// </summary>
        /// <returns>The current location as a <see cref="Location"/> object.</returns>
        public static async Task<Location> GetCurrentLocationAsync()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium);
                var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                    return location;
            }
            catch (FeatureNotSupportedException)
            {
                // Handle when the device does not support geolocation
            }
            catch (PermissionException)
            {
                // Handle when location permission is not granted
            }
            catch (Exception)
            {
                // Handle other possible exceptions
            }

            return null;
        }

        /// <summary>
        /// Decodes an encoded polyline string into a list of coordinates.
        /// </summary>
        /// <param name="encodedPolyline">The encoded polyline string.</param>
        /// <returns>A list of <see cref="Location"/> objects representing the decoded points.</returns>
        public static List<Location> DecodePolyline(string encodedPolyline)
        {
            if (string.IsNullOrWhiteSpace(encodedPolyline))
                return new List<Location>();

            var polylineChars = encodedPolyline.ToCharArray();
            var index = 0;
            var currentLatitude = 0;
            var currentLongitude = 0;

            var locations = new List<Location>();

            while (index < polylineChars.Length)
            {
                int result = 0;
                int shift = 0;
                int nextFiveBits;

                do
                {
                    nextFiveBits = polylineChars[index++] - 63;
                    result |= (nextFiveBits & 0x1F) << shift;
                    shift += 5;
                } while (nextFiveBits >= 0x20);

                int deltaLatitude = ((result & 1) != 0 ? ~(result >> 1) : (result >> 1));
                currentLatitude += deltaLatitude;

                result = 0;
                shift = 0;

                do
                {
                    nextFiveBits = polylineChars[index++] - 63;
                    result |= (nextFiveBits & 0x1F) << shift;
                    shift += 5;
                } while (nextFiveBits >= 0x20);

                int deltaLongitude = ((result & 1) != 0 ? ~(result >> 1) : (result >> 1));
                currentLongitude += deltaLongitude;

                locations.Add(new Location(currentLatitude / 1E5, currentLongitude / 1E5));
            }

            return locations;
        }

        // Method to calculate distance between two coordinates using Haversine formula
        public static double CalculateDistance(Location start, Location end)
        {
            const double EarthRadiusKm = 6371.0; // Radius of the Earth in kilometers

            var lat1 = DegreesToRadians(start.Latitude);
            var lon1 = DegreesToRadians(start.Longitude);
            var lat2 = DegreesToRadians(end.Latitude);
            var lon2 = DegreesToRadians(end.Longitude);

            var dLat = lat2 - lat1;
            var dLon = lon2 - lon1;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1) * Math.Cos(lat2) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = EarthRadiusKm * c; // Resulting distance in kilometers

            return distance;
        }

        // Helper function to convert degrees to radians
        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

    }
}