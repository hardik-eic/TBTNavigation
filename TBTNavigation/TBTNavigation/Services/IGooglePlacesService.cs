public interface IGooglePlacesService
{
    Task<List<string>> GetPlaceSuggestionsAsync(string input);
    Task<RouteInfo> GetRouteAsync(string origin, string destination);
}
