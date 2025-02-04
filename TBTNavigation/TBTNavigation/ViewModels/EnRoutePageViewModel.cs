using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls.Maps;
using TBTNavigation.Controls;
using TBTNavigation.Models;
using TBTNavigation.Helpers;
using TBTNavigation;
using GoogleMapsApi.Entities.Directions.Request;
using System.Text.RegularExpressions;

namespace TBTNavigation.ViewModels
{
    public class EnRoutePageViewModel : BaseViewModel
    {
        private readonly GooglePlacesService _placesService;
        public ObservableCollection<Location> RoutePoints { get; private set; }
        public ICommand ShowRouteCommand { get; }
        public ICommand NavigateCommand { get; }
        private readonly INavigation _navigation;
        private CancellationTokenSource _simulationCancellationTokenSource;

        private const double ThresholdDistance = 10; // Distance in meters

        private string _pickupLocation = String.Empty;
        public string PickupLocation
        {
            get => _pickupLocation;
            set
            {
                if (_pickupLocation != value)
                {
                    _pickupLocation = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _dropOffLocation = String.Empty;
        public string DropOffLocation
        {
            get => _dropOffLocation;
            set
            {
                if (_dropOffLocation != value)
                {
                    _dropOffLocation = value;
                    OnPropertyChanged();
                }
            }
        }

        private RouteInfo _routeInfoData;
        public RouteInfo RouteInfoData
        {
            get => _routeInfoData;
            set
            {
                SetProperty(ref _routeInfoData, value);
                OnPropertyChanged();
            }
        }

        private string _distance;
        public string Distance
        {
            get => _distance;
            set
            {
                SetProperty(ref _distance, value); // Using SetProperty from BaseViewModel
                UpdateIsDistanceAndTimeAvailable();
            }
        }

        private string _timeToReach;
        public string TimeToReach
        {
            get => _timeToReach;
            set
            {
                SetProperty(ref _timeToReach, value); // Using SetProperty from BaseViewModel
                UpdateIsDistanceAndTimeAvailable();
            }
        }

        private bool _isDistanceAndTimeAvailable;
        public bool IsDistanceAndTimeAvailable
        {
            get => _isDistanceAndTimeAvailable;
            set => SetProperty(ref _isDistanceAndTimeAvailable, value); // Using SetProperty from BaseViewModel
        }

        private Location? _deviceLocation;
        public Location? DeviceLocation
        {
            get => _deviceLocation;
            set => SetProperty(ref _deviceLocation, value);
        }

        private Location? _simulatedUserLocation;
        public Location? SimulatedUserLocation
        {
            get => _simulatedUserLocation;
            set => SetProperty(ref _simulatedUserLocation, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private List<MapPin> _pins;
        public List<MapPin> Pins
        {
            get { return _pins; }
            set { _pins = value; OnPropertyChanged(nameof(Pins)); }
        }

        private string _currentStepInstruction;
        public string CurrentStepInstruction
        {
            get => _currentStepInstruction;
            set
            {
                _currentStepInstruction = value;
                OnPropertyChanged(nameof(CurrentStepInstruction));
            }
        }

        private string _currentStepManeuver;
        public string CurrentStepManeuver
        {
            get => _currentStepManeuver;
            set
            {
                _currentStepManeuver = value;
                OnPropertyChanged(nameof(CurrentStepManeuver));
            }
        }

        private Maneuver _currentManeuver;
        public Maneuver CurrentManeuver
        {
            get => _currentManeuver;
            set
            {
                _currentManeuver = value;
                OnPropertyChanged(nameof(CurrentManeuver));
            }
        }

        private void UpdateIsDistanceAndTimeAvailable()
        {
            IsDistanceAndTimeAvailable = !string.IsNullOrEmpty(Distance) && !string.IsNullOrEmpty(TimeToReach);
        }

        public async Task ShowRouteAsync()
        {
            if (string.IsNullOrEmpty(PickupLocation) || string.IsNullOrEmpty(DropOffLocation))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please enter both pickup and drop-off locations.", "OK");
                return;
            }

            // Calculate the route details based on selected vehicle type
            await UpdateDistanceAndTimeAsync();

            // Get the route details from Google Places Service
            var route = await _placesService.GetRouteAsync(PickupLocation, DropOffLocation);
            if (route == null || route.RoutePoints == null || !route.RoutePoints.Any())
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Unable to calculate route. Please try again.", "OK");
                return;
            }

            // Update Distance and Duration
            Distance = route.DistanceText;
            TimeToReach = route.DurationText;
            OnPropertyChanged(nameof(Distance));
            OnPropertyChanged(nameof(TimeToReach));


            if (route != null)
            {
                RouteInfoData = route;
                OnPropertyChanged(nameof(RouteInfoData));
            }

            // Notify view with route points (optional: trigger event or binding for Map)
            RoutePoints = new ObservableCollection<Location>(
                route.RoutePoints.Select(point => new Location(point.Latitude, point.Longitude))
            );
            OnPropertyChanged(nameof(RoutePoints));
        }

        public void NavigateToMapPage()
        {
            _navigation.PushAsync(new EnRoutePage());
        }

        public async Task SimulateUserMovementAsync()
        {
            // Start simulation
            _simulationCancellationTokenSource = new CancellationTokenSource();

            try
            {
                if (RoutePoints == null || RoutePoints.Count == 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "No route available to simulate.", "OK");
                    return;
                }

                if (RouteInfoData?.StepInstructions == null || RouteInfoData.StepInstructions.Count == 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "No step instructions available.", "OK");
                    return;
                }

                int stepIndex = 0;
                string lastSpokenInstruction = null; // To track the last spoken instruction

                foreach (var point in RoutePoints)
                {
                    if (_simulationCancellationTokenSource.Token.IsCancellationRequested)
                        break;

                    SimulatedUserLocation = point; // Update the user's simulated location
                    OnPropertyChanged(nameof(SimulatedUserLocation));

                    // Update the current step instruction based on simulation progress
                    if (stepIndex < RouteInfoData.StepInstructions.Count)
                    {
                        CurrentStepInstruction = RouteInfoData.StepInstructions[stepIndex].HtmlInstructions;
                        Maneuver maneuver = RouteInfoData.StepInstructions[stepIndex].Maneuver ?? Maneuver.Unknown;
                        CurrentManeuver = maneuver;
                        CurrentStepManeuver = maneuver.ToCustomString();
                        OnPropertyChanged(nameof(CurrentStepInstruction));
                        OnPropertyChanged(nameof(CurrentStepManeuver));
                        OnPropertyChanged(nameof(CurrentManeuver));

                        // Check if the current location matches the step's end point
                        if (IsLocationNearStepEndPoint(point, stepIndex))
                        {
                            stepIndex++; // Move to the next step
                        }

                        // Speak the instruction only if it's different from the last one
                        string instructionText = ExtractTextFromHtml(CurrentStepInstruction);
                        if (instructionText != lastSpokenInstruction)
                        {
                            await TextToSpeech.Default.SpeakAsync(instructionText, new SpeechOptions
                            {
                                Volume = 1.0f,
                                Pitch = 1.0f,
                            });

                            lastSpokenInstruction = instructionText; // Update the last spoken instruction
                        }
                    }

                    await Task.Delay(3000, _simulationCancellationTokenSource.Token); // Wait for 3 seconds
                }

                if (!_simulationCancellationTokenSource.Token.IsCancellationRequested)
                {
                    await Application.Current.MainPage.DisplayAlert("Simulation", "User has reached the destination!", "OK");
                }
            }
            catch (TaskCanceledException ex)
            {
                // Simulation was canceled
                Console.WriteLine("Simulation was canceled.", ex);
            }
        }


        public string ExtractTextFromHtml(string html)
        {
            if (html == null)
            {
                return "";
            }

            string plainText = Regex.Replace(html, "<[^>]+?>", " ");
            plainText = System.Net.WebUtility.HtmlDecode(plainText).Trim();

            return plainText;
        }

        public async Task UpdateDistanceAndTimeAsync()
        {
            IsLoading = true;
            if (!string.IsNullOrEmpty(PickupLocation) && !string.IsNullOrEmpty(DropOffLocation))
            {
                var result = await _placesService.GetDistanceAndTimeAsync(PickupLocation, DropOffLocation);
                Distance = result.DistanceText;
                TimeToReach = result.DurationText;
                OnPropertyChanged(nameof(Distance));
                OnPropertyChanged(nameof(TimeToReach));
                IsLoading = false;
            }
        }

        private bool IsLocationNearStepEndPoint(Location point, int stepIndex)
        {
            // Replace with your logic to determine if the user has reached the step's end point
            // For example, you might compare the distance between `point` and the step's end location
            var stepEndPoint = RouteInfoData.StepInstructions[stepIndex].EndLocation; // Assuming you have StepEndpoints in your RouteInfoData
            return CalculateDistance(point, new Microsoft.Maui.Devices.Sensors.Location(stepEndPoint.Lat, stepEndPoint.Lng)) < ThresholdDistance; // ThresholdDistance is a small value like 10 meters
        }

        // private bool IsLocationNearStepEndPoint(Location point, int stepIndex)
        // {
        //     if (RouteInfoData?.StepEndpoints == null || stepIndex >= RouteInfoData.StepEndpoints.Count)
        //         return false;

        //     var stepEndPoint = RouteInfoData.StepEndpoints[stepIndex];

        //     // Calculate the distance between the current point and the step's endpoint
        //     double distance = CalculateDistance(point, stepEndPoint);

        //     // Return true if the distance is within the threshold
        //     return distance <= ThresholdDistance;
        // }

        private double CalculateDistance(Location location1, Location location2)
        {
            const double EarthRadiusKm = 6371; // Earth's radius in kilometers

            double lat1 = location1.Latitude * (Math.PI / 180);
            double lon1 = location1.Longitude * (Math.PI / 180);
            double lat2 = location2.Latitude * (Math.PI / 180);
            double lon2 = location2.Longitude * (Math.PI / 180);

            double deltaLat = lat2 - lat1;
            double deltaLon = lon2 - lon1;

            double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                       Math.Cos(lat1) * Math.Cos(lat2) *
                       Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            double distanceKm = EarthRadiusKm * c;

            // Convert kilometers to meters
            return distanceKm * 1000;
        }

        public EnRoutePageViewModel(INavigation navigation)
        {
            _navigation = navigation;
            _placesService = new GooglePlacesService(apiKey: "${MAPS_API_KEY}");
            ShowRouteCommand = new Command(async () => await ShowRouteAsync());
            NavigateCommand = new Command(() => NavigateToMapPage());
            Distance = "--";
            TimeToReach = "--";
        }
    }
}