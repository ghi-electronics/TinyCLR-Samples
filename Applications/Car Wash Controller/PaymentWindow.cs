using CarWashExample.Properties;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using SystemDrawing = System.Drawing;

namespace CarWashExample {
    internal sealed class PaymentWindow {
        private readonly Canvas canvas = new Canvas();
        private readonly SystemDrawing.Font fontNinaB = Resources.GetFont(Resources.FontResources.NinaB);

        public UIElement Elements { get; }

        public PaymentWindow() {
            // The on-screen keyboard is global state — keep it on a font that
            // matches the rest of this page so the popup doesn't visually
            // clash when the user taps into a TextBox.
            OnScreenKeyboard.Font = this.fontNinaB;
            this.Elements = this.CreatePage();
        }

        private UIElement CreatePage() {
            this.AddLabel("Input your credit card number :", 10,  20);
            this.AddLabel("Expire date :",                  132, 50);
            this.AddLabel("Pin :",                          187, 80);

            this.AddTextBox("#########", 250, 15);
            this.AddTextBox("01/01/2020", 250, 45);
            this.AddTextBox("0000",       250, 75);

            var backButton = MakeButton("Back");
            backButton.Click += (s, e) => Program.NavigateTo(Program.SelectServicePage.Elements);
            Canvas.SetLeft(backButton, 10);
            Canvas.SetTop(backButton, 220);
            this.canvas.Children.Add(backButton);

            var goButton = MakeButton("Next");
            goButton.Click += this.OnGoClick;
            Canvas.SetLeft(goButton, 370);
            Canvas.SetTop(goButton, 220);
            this.canvas.Children.Add(goButton);

            return this.canvas;
        }

        private void AddLabel(string text, int x, int y) {
            var label = new Text(this.fontNinaB, text) { ForeColor = Colors.White };
            Canvas.SetLeft(label, x);
            Canvas.SetTop(label, y);
            this.canvas.Children.Add(label);
        }

        private void AddTextBox(string initialText, int x, int y) {
            var textBox = new TextBox {
                Text = initialText,
                Font = this.fontNinaB,
                Width = 120,
                Height = 25,
            };
            Canvas.SetLeft(textBox, x);
            Canvas.SetTop(textBox, y);
            this.canvas.Children.Add(textBox);
        }

        private Button MakeButton(string label) => new Button {
            Child = new Text(this.fontNinaB, label) {
                ForeColor = Colors.Black,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
            Width = 100,
            Height = 40,
        };

        private void OnGoClick(object sender, RoutedEventArgs e) {
            var result = MessageBox.Show(this.Elements, "Are you sure?", "Confirm",
                MessageBox.MessageBoxButtons.YesNo, this.fontNinaB);

            if (result != MessageBox.DialogResult.Yes)
                return;

            Program.NavigateTo(Program.LoadingPage.Elements);
            Program.LoadingPage.Active();
        }
    }
}
