namespace TBTNavigation.Helpers
{
    public static class PolylineDecoder
    {
        public static List<Location> Decoder(string encodedPolyline)
        {
            if (string.IsNullOrEmpty(encodedPolyline))
                return null;

            var polylineChars = encodedPolyline.ToCharArray();
            var index = 0;
            var currentLat = 0;
            var currentLng = 0;
            var path = new List<Location>();

            while (index < polylineChars.Length)
            {
                // Decode latitude
                var shift = 0;
                var result = 0;
                int b;
                do
                {
                    b = polylineChars[index++] - 63;
                    result |= (b & 0x1f) << shift;
                    shift += 5;
                } while (b >= 0x20);
                currentLat += (result & 1) != 0 ? ~(result >> 1) : (result >> 1);

                // Decode longitude
                shift = 0;
                result = 0;
                do
                {
                    b = polylineChars[index++] - 63;
                    result |= (b & 0x1f) << shift;
                    shift += 5;
                } while (b >= 0x20);
                currentLng += (result & 1) != 0 ? ~(result >> 1) : (result >> 1);

                var lat = currentLat / 1E5;
                var lng = currentLng / 1E5;
                path.Add(new Location(lat, lng));
            }

            return path;
        }
    }

}