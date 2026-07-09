using GHIElectronics.TinyCLR.UI.Controls;
using SystemDrawing = System.Drawing;

namespace Demos {
    // PORTED TO THE UI DESIGNER.
    //
    // The screen body (the 7 information lines) is now designed in SystemInfoContent.tcui and built by the
    // SystemInfoContent control (a Canvas fragment). This window keeps its place in the demo's navigation
    // (still an ApplicationWindow, still registered in Program.Main) and simply hosts the designed content
    // and the shared TopBar chrome — so on-device behaviour is identical to the original hand-coded version.
    //
    // Compare with the original hand-coded SystemWindow.cs in the "SCM20260D Dev" project: the MakeLine()
    // loop and the fixed Canvas positions moved into the .tcui; only the hosting + runtime text remain.
    public class SystemWindow : ApplicationWindow {
        private Canvas canvas;

        public SystemWindow(SystemDrawing.Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
        }

        protected override void Active() {
            // Designer-built body (the 7 info lines, positioned in SystemInfoContent.tcui).
            this.canvas = new SystemInfoContent();

            // Shared chrome injected by ApplicationWindow — kept in code (gradient bar, clock, close button).
            if (this.TopBar != null) {
                Canvas.SetLeft(this.TopBar, 0);
                Canvas.SetTop(this.TopBar, 0);
                this.canvas.Children.Add(this.TopBar);
            }

            this.Child = this.canvas;
        }

        protected override void Deactive() => this.canvas.Children.Clear();
    }
}
