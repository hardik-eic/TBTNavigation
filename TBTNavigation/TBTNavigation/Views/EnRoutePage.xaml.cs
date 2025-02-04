using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using System.ComponentModel;
using TBTNavigation.ViewModels;
using TBTNavigation.Controls;
using TBTNavigation.Helpers;
using System.Threading.Tasks;
using TManeuver = TBTNavigation.Models.Maneuver;
namespace TBTNavigation;

public partial class EnRoutePage : ContentPage
{
    private readonly EnRoutePageViewModel _viewModel;

    public EnRoutePage()
    {
        InitializeComponent();
        Shell.SetNavBarIsVisible(this, false);
        _viewModel = new EnRoutePageViewModel(Navigation);
        BindingContext = _viewModel;
        _viewModel.Pins = new List<MapPin>();
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var geolocationRequest = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
        var location = await Geolocation.GetLocationAsync(geolocationRequest);
        MapView.MoveToRegion(MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(0.2)));
        MapView.CameraPosition = location;
        await GetRoute();
    }

    async Task GetRoute()
    {
        _viewModel.PickupLocation = "23.01590837413439, 72.47306989892792";
        _viewModel.DropOffLocation = "23.04361497986709, 72.47022320447037";
        await _viewModel.ShowRouteAsync();
        // await _viewModel.SimulateUserMovementAsync();
    }

    async Task<string> GetUserCurrentLocation()
    {
        // Request location
        var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest
        {
            DesiredAccuracy = GeolocationAccuracy.Best,
            Timeout = TimeSpan.FromSeconds(30)
        });

        if (location == null)
        {
            await DisplayAlert("Error", "Unable to fetch location", "OK");
            return String.Empty;
        }

        return location.ToString();
    }

    private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EnRoutePageViewModel.RoutePoints))
        {
            UpdateMap();
        }

        if (e.PropertyName == nameof(EnRoutePageViewModel.SimulatedUserLocation))
        {
            UpdateUserLocationPin();
        }
    }

    private void UpdateMap()
    {
        if (_viewModel.RoutePoints != null && _viewModel.RoutePoints.Any())
        {
            // Add polyline for the route
            var polyline = new Polyline
            {
                StrokeColor = Colors.Blue,
                StrokeWidth = 25
            };

            foreach (var point in _viewModel.RoutePoints)
            {
                polyline.Geopath.Add(point);
            }

            MapView.MapElements.Add(polyline);

            // Add pins for pickup and drop-off locations
            var pickup = new MapPin(MapPinClicked)
            {
                // Id = Guid.NewGuid().ToString(),
                Position = _viewModel.RoutePoints.First(),
                Label = "Pickup Location",
                Icon = "location",
                IconSrc = "location"
            };
            var dropoff = new MapPin(MapPinClicked)
            {
                // Id = Guid.NewGuid().ToString(),
                Position = _viewModel.RoutePoints.Last(),
                Label = "Dropoff Location",
                Icon = "arrival",
                IconSrc = "arrival"
            };

            _viewModel.Pins = [pickup, dropoff];

            // Adjust map to show the entire route
            var firstPoint = _viewModel.RoutePoints.First();
            var lastPoint = _viewModel.RoutePoints.Last();
            var distanceKm = LocationHelper.CalculateDistance(firstPoint, lastPoint);

            var mapSpan = MapSpan.FromCenterAndRadius(
                new Location(
                    (firstPoint.Latitude + lastPoint.Latitude) / 2,
                    (firstPoint.Longitude + lastPoint.Longitude) / 2),
                Distance.FromKilometers(distanceKm * 0.2)
            );

            // MapView.MoveToRegion(mapSpan);
        }
        else
        {
            DisplayAlert("Error", "Unable to display the route.", "OK");
        }
    }


    private void UpdateUserLocationPin()
    {
        if (_viewModel.SimulatedUserLocation == null)
            return;

        var userLocation = _viewModel.SimulatedUserLocation;

        // Clear existing user location pin
        var existingPin = MapView.Pins.FirstOrDefault(p => p.Label == "User Location");
        if (existingPin != null)
        {
            MapView.Pins.Remove(existingPin);
        }

        // Add new user location pin
        var userPin = new Pin
        {
            Label = "User Location",
            Location = userLocation,
            Type = PinType.Generic
        };

        MapView.Pins.Add(userPin);

        // Optionally center the map on the user's location
        MapView.MoveToRegion(MapSpan.FromCenterAndRadius(userLocation, Distance.FromKilometers(0.1)));

        // MapView.Bearing = (int)GetBearingForManeuver(_viewModel.CurrentManeuver, MapView.Bearing);
        // var cameraPosition = new Location(userLocation.Latitude, userLocation.Longitude);
        // MapView.CameraPosition = cameraPosition;
    }

    private float GetBearingForManeuver(TManeuver maneuver, float currentBearing)
    {
        return maneuver switch
        {
            TManeuver.TurnLeft => currentBearing - 90,
            TManeuver.TurnRight => currentBearing + 90,
            TManeuver.TurnSlightLeft => currentBearing - 45,
            TManeuver.TurnSlightRight => currentBearing + 45,
            TManeuver.TurnSharpLeft => currentBearing - 135,
            TManeuver.TurnSharpRight => currentBearing + 135,
            TManeuver.TurnU => currentBearing + 180,
            TManeuver.KeepRight => currentBearing + 15,
            TManeuver.KeepLeft => currentBearing - 15,
            TManeuver.Roundabout or TManeuver.RoundaboutExit => currentBearing + 45, // Approximate for roundabouts
            _ => currentBearing // Default: No change in bearing
        };
    }

    private void MapPinClicked(MapPin pin)
    {
        // ToDo: Handle here
    }
}

