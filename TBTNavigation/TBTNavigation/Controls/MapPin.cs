using System.Windows.Input;
using Microsoft.Maui.Controls.Maps;

namespace TBTNavigation.Controls
{
    public class MapPin
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public Location Position { get; set; }
        public string Icon { get; set; }
        public ICommand ClickedCommand { get; set; }
        public string IconSrc { get; set; }

        public MapPin(Action<MapPin> clicked)
        {
            ClickedCommand = new Command(() => clicked(this));
        }
    }
}
