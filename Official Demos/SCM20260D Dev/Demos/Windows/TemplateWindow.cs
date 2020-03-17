using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;

namespace Demos {
    public class TemplateWindow : ApplicationWindow  {
        private readonly Canvas canvas; // can be StackPanel

        public TemplateWindow(Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
            this.canvas = new Canvas();

            this.Element = this.canvas;

            this.CreateWindow();
        }

        private void CreateWindow() {            
            // Enable Top Bar
            this.canvas.Children.Add(this.TopBar.Element);
            this.TopBar.OnClose += this.Bar_OnClose;

            // Design your Window
            // .............

        }

        protected override void Active() {
            // To initialize, reset your variable, design...
        }

        protected override void Deactive() {
            // To stop or free, uinitialize variable resource
        }

        private void Bar_OnClose(object sender, GHIElectronics.TinyCLR.UI.RoutedEventArgs e) => this.Close();
    }
}
