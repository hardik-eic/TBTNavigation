using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using TBTNavigation.Models;
using TBTNavigation.Helpers;

public class GooglePlacesService : IGooglePlacesService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public GooglePlacesService(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient();
    }

    public async Task<List<string>> GetPlaceSuggestionsAsync(string input)
    {
        try
        {
            var url = $"https://maps.googleapis.com/maps/api/place/autocomplete/json?input={input}&key={_apiKey}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return new List<string>();

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var headings = new List<string>();
            var suggestions = new List<string>();

            foreach (var prediction in json.RootElement.GetProperty("predictions").EnumerateArray())
            {
                if (prediction.TryGetProperty("description", out var description))
                    suggestions.Add(description.GetString());
            }

            return suggestions;
        }
        catch
        {
            // Handle exceptions or logging as needed
            return new List<string>();
        }
    }

    public async Task<RouteInfo> GetRouteAsync(string origin, string destination)
    {
        try
        {
            var url = $"https://maps.googleapis.com/maps/api/directions/json?origin={Uri.EscapeDataString(origin)}&destination={Uri.EscapeDataString(destination)}&key={_apiKey}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            // var directions = JsonSerializer.Deserialize<GoogleDirectionsResponse>(json);
            var directions = GoogleDirectionsResponse.FromJson(json);


            if (directions?.Routes != null && directions.Routes.Any())
            {
                var route = directions.Routes.First();
                var points = PolylineDecoder.Decoder(route.OverviewPolyline.Points);

                var instructions = new List<Step>();
                foreach (var step in route.Legs.First().Steps)
                {
                    instructions.Add(new Step
                    {
                        Distance = step.Distance,
                        Duration = step.Duration,
                        HtmlInstructions = step.HtmlInstructions,
                        StartLocation = step.StartLocation,
                        EndLocation = step.EndLocation,
                        Maneuver = step.Maneuver,
                    });
                }

                return new RouteInfo
                {
                    DistanceText = route.Legs.First().Distance.Text,
                    DurationText = route.Legs.First().Duration.Text,
                    RoutePoints = points,
                    StepInstructions = instructions,
                };
            }
            else
            {
                return null;
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions or logging as needed
            await Application.Current.MainPage.DisplayAlert("Opps", ex.Message, "OK");
            return null;
        }
    }

    public async Task<(string DistanceText, string DurationText)> GetDistanceAndTimeAsync(string origin, string destination, string mode = "driving")
    {
        try
        {
            using var httpClient = new HttpClient();
            var url = $"https://maps.googleapis.com/maps/api/directions/json?origin={Uri.EscapeDataString(origin)}&destination={Uri.EscapeDataString(destination)}&mode={mode}&key={_apiKey}";

            var response = await httpClient.GetStringAsync(url);
            var jsonDoc = JsonDocument.Parse(response);

            var route = jsonDoc.RootElement.GetProperty("routes")[0];
            var leg = route.GetProperty("legs")[0];

            var distanceText = leg.GetProperty("distance").GetProperty("text").GetString();
            var durationText = leg.GetProperty("duration").GetProperty("text").GetString();

            return (distanceText, durationText);
        }
        catch (Exception ex)
        {
            // Handle errors (e.g., log them)
            return ("N/A", "N/A");
        }
    }
}

public class RouteInfo
{
    public string DistanceText { get; set; }
    public string DurationText { get; set; }
    public string PolylinePoints { get; set; }
    public List<Microsoft.Maui.Devices.Sensors.Location> RoutePoints { get; set; }
    public List<Step> StepInstructions { get; set; }

}
