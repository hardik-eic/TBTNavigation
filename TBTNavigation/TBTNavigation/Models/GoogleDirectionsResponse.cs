#nullable enable
#pragma warning disable CS8618
#pragma warning disable CS8601
#pragma warning disable CS8603

namespace TBTNavigation.Models
{
    using System;
    using System.Collections.Generic;

    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Globalization;

    public partial class GoogleDirectionsResponse
    {
        [JsonPropertyName("geocoded_waypoints")]
        public List<GeocodedWaypoint> GeocodedWaypoints { get; set; }

        [JsonPropertyName("routes")]
        public List<Route> Routes { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }

    public partial class GeocodedWaypoint
    {
        [JsonPropertyName("geocoder_status")]
        public string GeocoderStatus { get; set; }

        [JsonPropertyName("place_id")]
        public string PlaceId { get; set; }

        [JsonPropertyName("types")]
        public List<string> Types { get; set; }
    }

    public partial class Route
    {
        [JsonPropertyName("bounds")]
        public Bounds Bounds { get; set; }

        [JsonPropertyName("copyrights")]
        public string Copyrights { get; set; }

        [JsonPropertyName("legs")]
        public List<Leg> Legs { get; set; }

        [JsonPropertyName("overview_polyline")]
        public Polyline OverviewPolyline { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("warnings")]
        public List<object> Warnings { get; set; }

        [JsonPropertyName("waypoint_order")]
        public List<object> WaypointOrder { get; set; }
    }

    public partial class Bounds
    {
        [JsonPropertyName("northeast")]
        public Northeast Northeast { get; set; }

        [JsonPropertyName("southwest")]
        public Northeast Southwest { get; set; }
    }

    public partial class Northeast
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lng")]
        public double Lng { get; set; }
    }

    public partial class Leg
    {
        [JsonPropertyName("distance")]
        public Distance Distance { get; set; }

        [JsonPropertyName("duration")]
        public Distance Duration { get; set; }

        [JsonPropertyName("end_address")]
        public string EndAddress { get; set; }

        [JsonPropertyName("end_location")]
        public Northeast EndLocation { get; set; }

        [JsonPropertyName("start_address")]
        public string StartAddress { get; set; }

        [JsonPropertyName("start_location")]
        public Northeast StartLocation { get; set; }

        [JsonPropertyName("steps")]
        public List<Step> Steps { get; set; }

        [JsonPropertyName("traffic_speed_entry")]
        public List<object> TrafficSpeedEntry { get; set; }

        [JsonPropertyName("via_waypoint")]
        public List<object> ViaWaypoint { get; set; }
    }

    public partial class Distance
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("value")]
        public long Value { get; set; }
    }

    public partial class Step
    {
        [JsonPropertyName("distance")]
        public Distance Distance { get; set; }

        [JsonPropertyName("duration")]
        public Distance Duration { get; set; }

        [JsonPropertyName("end_location")]
        public Northeast EndLocation { get; set; }

        [JsonPropertyName("html_instructions")]
        public string HtmlInstructions { get; set; }

        [JsonPropertyName("polyline")]
        public Polyline Polyline { get; set; }

        [JsonPropertyName("start_location")]
        public Northeast StartLocation { get; set; }

        [JsonPropertyName("travel_mode")]
        public TravelMode TravelMode { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("maneuver")]
        public Maneuver? Maneuver { get; set; }
    }

    public partial class Polyline
    {
        [JsonPropertyName("points")]
        public string Points { get; set; }
    }

    public enum Maneuver
    {
        Start,
        KeepLeft,
        KeepRight,
        TurnLeft,
        TurnRight,
        TurnSlightLeft,
        TurnSlightRight,
        TurnSharpLeft,
        TurnSharpRight,
        TurnU,
        Continue,
        Roundabout,
        RoundaboutExit,
        Merge,
        ForkLeft,
        ForkRight,
        Ramp,
        RampExit,
        RampLeft,
        RampRight,
        TurnLeftLeft,
        TurnRightRight,
        TurnLeftRight,
        TurnRightLeft,
        TurnLeftSharpLeft,
        TurnRightSharpRight,
        TurnLeftSlightLeft,
        TurnRightSlightRight,
        TurnLeftSharpRight,
        TurnRightSharpLeft,
        TurnLeftSlightRight,
        TurnRightSlightLeft,
        TurnLeftU,
        TurnRightU,
        TurnLeftRoundabout,
        TurnRightRoundabout,
        TurnLeftRoundaboutExit,
        Unknown,
    }

    public enum TravelMode { Driving };

    public partial class GoogleDirectionsResponse
    {
        public static GoogleDirectionsResponse FromJson(string json) => JsonSerializer.Deserialize<GoogleDirectionsResponse>(json, TBTNavigation.Models.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this GoogleDirectionsResponse self) => JsonSerializer.Serialize(self, TBTNavigation.Models.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerOptions Settings = new(JsonSerializerDefaults.General)
        {
            Converters =
            {
                ManeuverConverter.Singleton,
                TravelModeConverter.Singleton,
                new DateOnlyConverter(),
                new TimeOnlyConverter(),
                IsoDateTimeOffsetConverter.Singleton
            },
        };
    }

    internal class ManeuverConverter : JsonConverter<Maneuver>
    {
        public override bool CanConvert(Type t) => t == typeof(Maneuver);

        public override Maneuver Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString()?.ToLowerInvariant(); // Normalize case
            Console.WriteLine("Maneuver Direction: " + value);
            return value switch
            {
                "keep-right" => Maneuver.KeepRight,
                "turn-left" => Maneuver.TurnLeft,
                "turn-right" => Maneuver.TurnRight,
                _ => Maneuver.Unknown
            };
        }

        public override void Write(Utf8JsonWriter writer, Maneuver value, JsonSerializerOptions options)
        {
            var stringValue = value switch
            {
                Maneuver.KeepRight => "keep-right",
                Maneuver.TurnLeft => "turn-left",
                Maneuver.TurnRight => "turn-right",
                _ => "Unknown"
            };

            if (stringValue != null)
            {
                writer.WriteStringValue(stringValue);
            }
        }

        public static readonly ManeuverConverter Singleton = new ManeuverConverter();
    }

    internal class TravelModeConverter : JsonConverter<TravelMode>
    {
        public override bool CanConvert(Type t) => t == typeof(TravelMode);

        public override TravelMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (value == "DRIVING")
            {
                return TravelMode.Driving;
            }
            throw new Exception("Cannot unmarshal type TravelMode");
        }

        public override void Write(Utf8JsonWriter writer, TravelMode value, JsonSerializerOptions options)
        {
            if (value == TravelMode.Driving)
            {
                JsonSerializer.Serialize(writer, "DRIVING", options);
                return;
            }
            throw new Exception("Cannot marshal type TravelMode");
        }

        public static readonly TravelModeConverter Singleton = new TravelModeConverter();
    }

    public class DateOnlyConverter : JsonConverter<DateOnly>
    {
        private readonly string serializationFormat;
        public DateOnlyConverter() : this(null) { }

        public DateOnlyConverter(string? serializationFormat)
        {
            this.serializationFormat = serializationFormat ?? "yyyy-MM-dd";
        }

        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return DateOnly.Parse(value!);
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
                => writer.WriteStringValue(value.ToString(serializationFormat));
    }

    public class TimeOnlyConverter : JsonConverter<TimeOnly>
    {
        private readonly string serializationFormat;

        public TimeOnlyConverter() : this(null) { }

        public TimeOnlyConverter(string? serializationFormat)
        {
            this.serializationFormat = serializationFormat ?? "HH:mm:ss.fff";
        }

        public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return TimeOnly.Parse(value!);
        }

        public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
                => writer.WriteStringValue(value.ToString(serializationFormat));
    }

    internal class IsoDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        public override bool CanConvert(Type t) => t == typeof(DateTimeOffset);

        private const string DefaultDateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";

        private DateTimeStyles _dateTimeStyles = DateTimeStyles.RoundtripKind;
        private string? _dateTimeFormat;
        private CultureInfo? _culture;

        public DateTimeStyles DateTimeStyles
        {
            get => _dateTimeStyles;
            set => _dateTimeStyles = value;
        }

        public string? DateTimeFormat
        {
            get => _dateTimeFormat ?? string.Empty;
            set => _dateTimeFormat = (string.IsNullOrEmpty(value)) ? null : value;
        }

        public CultureInfo Culture
        {
            get => _culture ?? CultureInfo.CurrentCulture;
            set => _culture = value;
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            string text;


            if ((_dateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal
                    || (_dateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal)
            {
                value = value.ToUniversalTime();
            }

            text = value.ToString(_dateTimeFormat ?? DefaultDateTimeFormat, Culture);

            writer.WriteStringValue(text);
        }

        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? dateText = reader.GetString();

            if (string.IsNullOrEmpty(dateText) == false)
            {
                if (!string.IsNullOrEmpty(_dateTimeFormat))
                {
                    return DateTimeOffset.ParseExact(dateText, _dateTimeFormat, Culture, _dateTimeStyles);
                }
                else
                {
                    return DateTimeOffset.Parse(dateText, Culture, _dateTimeStyles);
                }
            }
            else
            {
                return default(DateTimeOffset);
            }
        }


        public static readonly IsoDateTimeOffsetConverter Singleton = new IsoDateTimeOffsetConverter();
    }
}
#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603
