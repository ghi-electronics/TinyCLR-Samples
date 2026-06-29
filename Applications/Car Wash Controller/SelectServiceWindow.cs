using System;
using CarWashExample.Properties;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;
using GHIElectronics.TinyCLR.UI.Shapes;
using SystemDrawing = System.Drawing;

namespace CarWashExample {
    internal sealed class SelectServiceWindow {
        private readonly Canvas canvas = new Canvas();
        private readonly SystemDrawing.Font fontNinaB = Resources.GetFont(Resources.FontResources.NinaB);
        private readonly SystemDrawing.Font fontDroid12 = Resources.GetFont(Resources.FontResources.droid_reg12);
        private readonly SystemDrawing.Font fontDroid14 = Resources.GetFont(Resources.FontResources.droid_reg14);

        public UIElement Elements { get; }

        public SelectServiceWindow() => this.Elements = this.CreatePage();

        private UIElement CreatePage() {
            const int xButton = 300, yButtonStart = 15, yButtonStep = 60;
            const int xPriceText = 420, yPriceStart = 25, yPriceStep = 60;
            const int xDividerLine = 280;

            this.AddServiceRow("Premium",  "4.99$", xButton, yButtonStart + yButtonStep * 0, xPriceText, yPriceStart + yPriceStep * 0);
            this.AddServiceRow("Standard", "3.99$", xButton, yButtonStart + yButtonStep * 1, xPriceText, yPriceStart + yPriceStep * 1);
            this.AddServiceRow("Basic",    "2.99$", xButton, yButtonStart + yButtonStep * 2, xPriceText, yPriceStart + yPriceStep * 2);
            this.AddServiceRow("Free",     "0.00$", xButton, yButtonStart + yButtonStep * 3, xPriceText, yPriceStart + yPriceStep * 3);

            // Vertical divider between the vehicle list (left) and the
            // service-tier buttons (right).
            var divider = new Line(0, 250) { Stroke = new Pen(Colors.Yellow) };
            Canvas.SetLeft(divider, xDividerLine);
            Canvas.SetTop(divider, 10);
            this.canvas.Children.Add(divider);

            // Date in the bottom-left corner.
            var dateText = new Text(this.fontNinaB,
                DateTime.Now.Day + " / " + DateTime.Now.Month + " / " + DateTime.Now.Year) {
                ForeColor = Colors.White,
            };
            Canvas.SetLeft(dateText, 100);
            Canvas.SetTop(dateText, 250);
            this.canvas.Children.Add(dateText);

            var prompt = new Text(this.fontNinaB, "Select Vehicle Type:") { ForeColor = Colors.White };
            Canvas.SetLeft(prompt, 10);
            Canvas.SetTop(prompt, yPriceStart);
            this.canvas.Children.Add(prompt);

            var vehicleList = new ListBox();
            vehicleList.Items.Add(MakeVehicleItem("Truck"));
            vehicleList.Items.Add(MakeVehicleItem("Van"));
            vehicleList.Items.Add(MakeVehicleItem("SUV"));
            vehicleList.Items.Add(MakeVehicleItem("Sedan"));
            vehicleList.Items.Add(MakeVehicleItem("Other"));
            vehicleList.SelectedIndex = 0;
            Canvas.SetLeft(vehicleList, 160);
            Canvas.SetTop(vehicleList, yPriceStart);
            this.canvas.Children.Add(vehicleList);

            // Accepted-payment logos along the bottom of the vehicle column.
            this.AddPaymentLogo(Resources.BitmapResources.visa,   20,  yPriceStart + 150);
            this.AddPaymentLogo(Resources.BitmapResources.master, 100, yPriceStart + 150);
            this.AddPaymentLogo(Resources.BitmapResources.paypal, 180, yPriceStart + 150);

            return this.canvas;
        }

        private void AddServiceRow(string label, string price, int xButton, int yButton, int xPrice, int yPrice) {
            var button = new Button {
                Child = new Text(this.fontDroid12, label) {
                    ForeColor = Colors.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                Width = 100,
                Height = 40,
            };
            button.Click += this.OnServiceButtonClick;

            Canvas.SetLeft(button, xButton);
            Canvas.SetTop(button, yButton);
            this.canvas.Children.Add(button);

            var priceText = new Text(this.fontDroid14, price) { ForeColor = Colors.White };
            Canvas.SetLeft(priceText, xPrice);
            Canvas.SetTop(priceText, yPrice);
            this.canvas.Children.Add(priceText);
        }

        private ListBoxItemHighlightable MakeVehicleItem(string text) =>
            new ListBoxItemHighlightable(text, this.fontNinaB, 4, Colors.Blue, Colors.White, Colors.White);

        private void AddPaymentLogo(Resources.BitmapResources resource, int x, int y) {
            var image = new Image {
                Source = BitmapImage.FromGraphics(SystemDrawing.Graphics.FromImage(Resources.GetBitmap(resource))),
            };
            Canvas.SetLeft(image, x);
            Canvas.SetTop(image, y);
            this.canvas.Children.Add(image);
        }

        private void OnServiceButtonClick(object sender, RoutedEventArgs e) =>
            Program.NavigateTo(Program.PaymentPage.Elements);
    }
}
