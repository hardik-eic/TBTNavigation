using TBTNavigation.Models;

namespace TBTNavigation.Helpers
{
    static class ManeuverEx
    {
        public static string ToCustomString(this Maneuver maneuver)
        {
            return maneuver switch
            {
                Maneuver.Start => "Start",
                Maneuver.KeepRight => "Keep right",
                Maneuver.TurnLeft => "Turn left",
                Maneuver.TurnRight => "Turn right",
                Maneuver.TurnSlightLeft => "Turn slight left",
                Maneuver.TurnSlightRight => "Turn slight right",
                Maneuver.TurnSharpLeft => "Turn sharp left",
                Maneuver.TurnSharpRight => "Turn sharp right",
                Maneuver.TurnU => "Turn U",
                Maneuver.Continue => "Continue",
                Maneuver.Roundabout => "Roundabout",
                Maneuver.RoundaboutExit => "Roundabout exit",
                Maneuver.Merge => "Merge",
                Maneuver.ForkLeft => "Fork left",
                Maneuver.ForkRight => "Fork right",
                Maneuver.Ramp => "Ramp",
                Maneuver.RampExit => "Ramp exit",
                Maneuver.RampLeft => "Ramp left",
                Maneuver.RampRight => "Ramp right",
                Maneuver.TurnLeftLeft => "Turn left left",
                Maneuver.TurnRightRight => "Turn right right",
                Maneuver.TurnLeftRight => "Turn left right",
                Maneuver.TurnRightLeft => "Turn right left",
                Maneuver.TurnLeftSharpLeft => "Turn left sharp left",
                Maneuver.TurnRightSharpRight => "Turn right sharp right",
                Maneuver.TurnLeftSlightLeft => "Turn left slight left",
                Maneuver.TurnRightSlightRight => "Turn right slight right",
                Maneuver.TurnLeftSharpRight => "Turn left sharp right",
                Maneuver.TurnRightSharpLeft => "Turn right sharp left",
                Maneuver.TurnLeftSlightRight => "Turn left slight right",
                Maneuver.TurnRightSlightLeft => "Turn right slight left",
                Maneuver.TurnLeftU => "Turn left U",
                Maneuver.TurnRightU => "Turn right U",
                Maneuver.TurnLeftRoundabout => "Turn left roundabout",
                Maneuver.TurnRightRoundabout => "Turn right roundabout",
                Maneuver.TurnLeftRoundaboutExit => "Turn left roundabout exit",
                _ => "Unknown",
            };
        }
    }
}