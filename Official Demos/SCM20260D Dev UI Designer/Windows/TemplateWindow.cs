using System.Drawing;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;

namespace Demos {
    public class TemplateWindow : ApplicationWindow {
        private Canvas canvas;

        public TemplateWindow(Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
        }

        protected override void Active() {
            this.canvas = new Canvas();
            this.Child = this.canvas;

            // Top bar.
            if (this.TopBar != null) {
                Canvas.SetLeft(this.TopBar, 0);
                Canvas.SetTop(this.TopBar, 0);
                this.canvas.Children.Add(this.TopBar);
            }

            // Bottom bar with Back/Next, if enabled by the caller.
            if (this.BottomBar != null) {
                Canvas.SetLeft(this.BottomBar, 0);
                Canvas.SetTop(this.BottomBar, this.Height - this.BottomBar.Height);
                this.canvas.Children.Add(this.BottomBar);

                this.OnBottomBarButtonBackTouchUpEvent += this.OnButtonBack;
                this.OnBottomBarButtonNextTouchUpEvent += this.OnButtonNext;
            }
        }

        private void OnButtonBack(object sender, RoutedEventArgs e) => this.Close();
        private void OnButtonNext(object sender, RoutedEventArgs e) => this.Close();

        protected override void Deactive() => this.canvas.Children.Clear();
    }
}
