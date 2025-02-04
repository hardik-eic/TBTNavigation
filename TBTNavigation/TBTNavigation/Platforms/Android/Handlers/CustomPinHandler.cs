using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Hardware;
using GoogleMapsApi.StaticMaps.Enums;
using Microsoft.Maui.Maps.Handlers;
using TBTNavigation.Controls;
namespace TBTNavigation.Platforms.Android;

public class CustomMapHandler : MapHandler
{
    public GoogleMap _googleMap;
    private const int _iconSize = 120;

    private readonly Dictionary<string, BitmapDescriptor> _iconMap = [];

    public static new IPropertyMapper<MapEx, CustomMapHandler> Mapper = new PropertyMapper<MapEx, CustomMapHandler>(MapHandler.Mapper)
    {
        [nameof(MapEx.CustomPins)] = MapPins,
        [nameof(MapEx.CompassEnabled)] = MapCompassEnabled,
        [nameof(MapEx.MapType)] = SetMapType,
        [nameof(MapEx.CameraPosition)] = MapCameraPosition,
        // [nameof(MapEx.MapDetails)] = SetMapDetails,
    };

    public Dictionary<string, (Marker Marker, MapPin Pin)> MarkerMap { get; } = [];

    public CustomMapHandler() : base(Mapper)
    {
    }

    protected override void ConnectHandler(MapView platformView)
    {
        base.ConnectHandler(platformView);
        var mapReady = new MapCallbackHandler(this);

        PlatformView.GetMapAsync(mapReady);
    }

    private static void MapCompassEnabled(CustomMapHandler handler, MapEx map)
    {
        if (handler._googleMap != null)
        {
            handler._googleMap.UiSettings.CompassEnabled = map.CompassEnabled;
        }
    }

    private static new void MapPins(IMapHandler handler, Microsoft.Maui.Maps.IMap map)
    {
        if (handler.Map is null || handler.MauiContext is null)
        {
            return;
        }

        if (handler is CustomMapHandler mapHandler)
        {
            foreach (var marker in mapHandler.MarkerMap)
            {
                marker.Value.Marker.Remove();
            }

            mapHandler.MarkerMap.Clear();

            mapHandler.AddPins();
        }
    }

    private BitmapDescriptor GetIcon(string icon)
    {
        if (_iconMap.TryGetValue(icon, out BitmapDescriptor? value))
        {
            return value;
        }

        var drawable = Context.Resources.GetIdentifier(icon, "drawable", Context.PackageName);
        var bitmap = BitmapFactory.DecodeResource(Context.Resources, drawable);
        var scaled = Bitmap.CreateScaledBitmap(bitmap, _iconSize, _iconSize, false);
        bitmap.Recycle();
        var descriptor = BitmapDescriptorFactory.FromBitmap(scaled);

        _iconMap[icon] = descriptor;

        return descriptor;
    }

    private void AddPins()
    {
        if (VirtualView is MapEx mapEx && mapEx.CustomPins != null)
        {
            foreach (var pin in mapEx.CustomPins)
            {
                var markerOption = new MarkerOptions();
                markerOption.SetTitle(string.Empty);
                markerOption.SetIcon(GetIcon(pin.IconSrc));
                markerOption.SetPosition(new LatLng(pin.Position.Latitude, pin.Position.Longitude));
                var marker = Map.AddMarker(markerOption);

                MarkerMap.Add(marker.Id, (marker, pin));
            }
        }
    }

    public void MarkerClick(object sender, GoogleMap.MarkerClickEventArgs args)
    {
        if (MarkerMap.TryGetValue(args.Marker.Id, out (Marker Marker, MapPin Pin) value))
        {
            value.Pin.ClickedCommand?.Execute(null);
        }
    }

    private static void SetMapType(CustomMapHandler handler, MapEx map)
    {
        if (handler._googleMap != null)
        {
            handler._googleMap.MapType = map.MapType switch
            {
                MapType.Roadmap => GoogleMap.MapTypeNormal,
                MapType.Satellite => GoogleMap.MapTypeSatellite,
                MapType.Terrain => GoogleMap.MapTypeTerrain,
                MapType.Hybrid => GoogleMap.MapTypeHybrid,
                _ => GoogleMap.MapTypeNormal
            };
        }
    }

    private static void MapCameraPosition(CustomMapHandler handler, MapEx customMap)
    {
        if (handler._googleMap != null && customMap.CameraPosition != null)
        {
            var position = new LatLng(customMap.CameraPosition.Latitude, customMap.CameraPosition.Longitude);
            // var bearing = customMap.Bearing;
            var cameraUpdate = CameraUpdateFactory.NewLatLngZoom(position, 18); // Default zoom is 15
            CameraPosition cameraPosition = new CameraPosition.Builder()
            .Target(position)
            .Zoom(18)
            .Bearing(45)
            .Tilt(30)
            .Build();
            handler._googleMap.MoveCamera(cameraUpdate);
            handler._googleMap.AnimateCamera(CameraUpdateFactory.NewCameraPosition(cameraPosition));
        }
    }

    // private static void SetMapDetails(CustomMapHandler handler, MapEx map)
    // {
    //     if (handler._googleMap != null)
    //     {
    //         // Map details feature (Traffic, Transit, etc.)
    //         switch (map.MapDetails?.ToLower())
    //         {
    //             case "traffic":
    //                 handler._googleMap.TrafficEnabled = !handler._googleMap.TrafficEnabled;
    //                 break;
    //             case "transit":
    //                 handler._googleMap.SetTransitLayerEnabled(true);
    //                 break;
    //             case "bicycling":
    //                 handler._googleMap.SetBicycleLayerEnabled(true);
    //                 break;
    //                 // Add more details as needed
    //         }
    //     }
    // }
}

public class MapCallbackHandler : Java.Lang.Object, IOnMapReadyCallback
{
    private readonly CustomMapHandler mapHandler;

    public MapCallbackHandler(CustomMapHandler mapHandler)
    {
        this.mapHandler = mapHandler;
    }

    public void OnMapReady(GoogleMap googleMap)
    {
        mapHandler.UpdateValue(nameof(MapEx.CustomPins));
        googleMap.MapType = GoogleMap.MapTypeTerrain;

        mapHandler._googleMap = googleMap;
        mapHandler._googleMap.MyLocationEnabled = true;
        mapHandler._googleMap.UiSettings.MyLocationButtonEnabled = true;
        mapHandler._googleMap.UiSettings.ZoomControlsEnabled = true;
        googleMap.UiSettings.CompassEnabled = (this.mapHandler.VirtualView as MapEx)?.CompassEnabled ?? true;

        //     googleMap.MarkerClick += mapHandler.MarkerClick;
        //     googleMap.CameraIdle += (sender, args) =>
        //    {
        //        var center = googleMap.CameraPosition.Target;
        //        (this.mapHandler.VirtualView as MapEx).CameraPosition = new Location(center.Latitude, center.Longitude);
        //    };
    }
}