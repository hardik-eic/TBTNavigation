using GoogleMapsApi.StaticMaps.Enums;

namespace TBTNavigation.Controls
{
    public class MapEx : Microsoft.Maui.Controls.Maps.Map
    {
        public List<MapPin> CustomPins
        {
            get { return (List<MapPin>)GetValue(CustomPinsProperty); }
            set { SetValue(CustomPinsProperty, value); }
        }
        public static readonly BindableProperty CustomPinsProperty = BindableProperty.Create(nameof(CustomPins), typeof(List<MapPin>), typeof(MapEx), null);

        public bool CompassEnabled
        {
            get => (bool)GetValue(CompassEnabledProperty);
            set => SetValue(CompassEnabledProperty, value);
        }
        public static readonly BindableProperty CompassEnabledProperty =
           BindableProperty.Create(nameof(CompassEnabled), typeof(bool), typeof(MapEx), true);

        public MapType MapType
        {
            get => (MapType)GetValue(MapTypeProperty);
            set => SetValue(MapTypeProperty, value);
        }
        public static readonly BindableProperty MapTypeProperty = BindableProperty.Create(
            nameof(MapType), typeof(MapType), typeof(MapEx), MapType.Terrain);

        public string MapDetails
        {
            get => (string)GetValue(MapDetailsProperty);
            set => SetValue(MapDetailsProperty, value);
        }
        public static readonly BindableProperty MapDetailsProperty = BindableProperty.Create(
            nameof(MapDetails), typeof(string), typeof(MapEx), default(string));

        public int Zoom
        {
            get => (int)GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }
        public static readonly BindableProperty ZoomProperty = BindableProperty.Create(
            nameof(Zoom), typeof(int), typeof(MapEx), default(int));

        public int Bearing
        {
            get => (int)GetValue(BearingProperty);
            set => SetValue(BearingProperty, value);
        }
        public static readonly BindableProperty BearingProperty = BindableProperty.Create(
            nameof(Bearing), typeof(int), typeof(MapEx), default(int));

        public int Tilt
        {
            get => (int)GetValue(TiltProperty);
            set => SetValue(TiltProperty, value);
        }
        public static readonly BindableProperty TiltProperty = BindableProperty.Create(
            nameof(Tilt), typeof(int), typeof(MapEx), default(int));

        public Location CameraPosition
        {
            get => (Location)GetValue(CameraPositionProperty);
            set => SetValue(CameraPositionProperty, value);
        }

        public static readonly BindableProperty CameraPositionProperty =
        BindableProperty.Create(
        nameof(CameraPosition),
        typeof(Location),
        typeof(MapEx),
        null,
        propertyChanged: OnCameraPositionChanged);

        private static void OnCameraPositionChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MapEx customMap && newValue is Location newLocation)
            {
                customMap.Handler?.UpdateValue(nameof(CameraPosition));
            }
        }
    }
}

