using CarWashExample.Properties;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using SystemDrawing = System.Drawing;

namespace CarWashExample {
    internal sealed class EndPage {
        private readonly Canvas canvas = new Canvas();
        private readonly SystemDrawing.Font font = Resources.GetFont(Resources.FontResources.NinaB);

        public UIElement Elements { get; }

        public EndPage() => this.Elements = this.CreatePage();

        public void Active() { }
        public void Deactive() { }

        private UIElement CreatePage() {
            this.AddLabel("Will you:", 10, 10);

            // CheckBox + three radio buttons stacked vertically with labels.
            var checkbox = new CheckBox();
            Canvas.SetLeft(checkbox, 50);
            Canvas.SetTop(checkbox, 50);
            this.canvas.Children.Add(checkbox);
            this.AddLabel("Back to us.", 50 + checkbox.Width + 5, 50);

            this.AddRadioOption("radio1", "Sell your vehicle.",   50, 100);
            this.AddRadioOption("radio2", "Donate your vehicle.", 50, 150);
            this.AddRadioOption("radio3", "Not sure yet.",        50, 200);

            var doneButton = new Button {
                Child = new Text(this.font, "Done!") {
                    ForeColor = Colors.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                Width = 100,
                Height = 40,
            };
            doneButton.Click += (s, e) => Program.NavigateTo(Program.SelectServicePage.Elements);
            Canvas.SetLeft(doneButton, 370);
            Canvas.SetTop(doneButton, 220);
            this.canvas.Children.Add(doneButton);

            return this.canvas;
        }

        private void AddLabel(string text, int x, int y) {
            var label = new Text(this.font, text) { ForeColor = Colors.White };
            Canvas.SetLeft(label, x);
            Canvas.SetTop(label, y);
            this.canvas.Children.Add(label);
        }

        private void AddRadioOption(string name, string text, int x, int y) {
            var radio = new RadioButton { Name = name };
            Canvas.SetLeft(radio, x);
            Canvas.SetTop(radio, y);
            this.canvas.Children.Add(radio);
            this.AddLabel(text, x + radio.Width + 5, y);
        }
    }
}
