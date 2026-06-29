using Demos.Properties;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;

namespace Demos {
    internal class TemplateWindow : ApplicationWindow {
        private Canvas canvas;

        public TemplateWindow(Resources.BitmapResources icon, string text, int width, int height) : base(icon, text, width, height) {
        }

        protected override void Active() {
            this.canvas = new Canvas();
            this.Child = this.canvas;

            if (this.TopBar != null) {
                Canvas.SetLeft(this.TopBar, 0);
                Canvas.SetTop(this.TopBar, 0);
                this.canvas.Children.Add(this.TopBar);
            }

            if (this.BottomBar != null) {
                Canvas.SetLeft(this.BottomBar, 0);
                Canvas.SetTop(this.BottomBar, this.Height - this.BottomBar.Height);
                this.canvas.Children.Add(this.BottomBar);

                this.OnBottomBarButtonUpEvent += this.OnHardwareButtonUp;
            }
        }

        private void OnHardwareButtonUp(object sender, RoutedEventArgs e) {
            var buttonSource = (ButtonEventArgs)e;

            switch (buttonSource.Button) {
                case HardwareButton.Left:
                    // Close this window and return to the previous one.
                    this.Close();
                    break;
                case HardwareButton.Right:
                    // Your "next screen" action here.
                    break;
                case HardwareButton.Select:
                    // Your "select / OK" action here.
                    break;
            }
        }

        protected override void Deactive() {
            if (this.BottomBar != null)
                this.OnBottomBarButtonUpEvent -= this.OnHardwareButtonUp;

            this.canvas?.Children.Clear();
        }
    }
}
