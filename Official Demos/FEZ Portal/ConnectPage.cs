using System;
using System.Drawing;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using FEZ_Portal.Properties;

namespace FEZ_Portal {
    public sealed class ConnectPage {

        private Canvas canvas;
        private Font font;
        private Font fontB;
        private string address;
        public UIElement Elements { get; }
        public ConnectPage(string ip) {
            this.canvas = new Canvas();
            this.font = Resources.GetFont(Resources.FontResources.NinaB);
            this.fontB = Resources.GetFont(Resources.FontResources.ArialBlack);
            this.address = ip;
            this.Elements = this.CreatePage();
        }
        private UIElement CreatePage() {

            var dateText = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, DateTime.Now.Day + " / " + DateTime.Now.Month + " / " + DateTime.Now.Year) {
                ForeColor = Colors.White,
            };
            Canvas.SetLeft(dateText, 390);
            Canvas.SetTop(dateText, 10);
            this.canvas.Children.Add(dateText);

            var addressText = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, this.address) {
                ForeColor = Colors.White,
            };
            Canvas.SetLeft(addressText, 10);
            Canvas.SetTop(addressText, 10);
            this.canvas.Children.Add(addressText);

            var connectedText = new GHIElectronics.TinyCLR.UI.Controls.Text(this.fontB, "CONNECTED") {
                ForeColor = Colors.White,
            };
            Canvas.SetLeft(connectedText, 160);
            Canvas.SetTop(connectedText, 115);
            this.canvas.Children.Add(connectedText);

            return this.canvas;
        }
    }
}
