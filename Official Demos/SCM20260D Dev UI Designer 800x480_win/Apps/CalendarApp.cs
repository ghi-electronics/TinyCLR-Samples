using GHIElectronics.TinyCLR.UI.Controls;

namespace Demos {
    // Date & Time app fragment. Layout is in Apps\CalendarApp.tcui; the Calendar shows a
    // fake RTC date and the clock/date Text are static labels. No timer here — the Desktop
    // shell drives any live updates.
    public partial class CalendarApp : Canvas {
        public CalendarApp() => InitializeComponent();
    }
}
