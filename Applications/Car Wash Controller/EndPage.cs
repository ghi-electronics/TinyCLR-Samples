using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;

using CarWashExample.Properties;

namespace CarWashExample
{
    public sealed class EndPage
    {
        private Canvas canvas;        
        private Font fontB;

        public UIElement Elements { get; }

        public EndPage()
        {
            this.canvas = new Canvas();
            this.fontB = Resources.GetFont(Resources.FontResources.NinaB);
            this.Elements = this.CreatePage();
        }
        public void Active()
        {
            // Initialize something
        }

        public void Deactive()
        {
            // Deinit something
        }

        private UIElement CreatePage()
        {
            this.canvas.Children.Clear();

            var willYouText = new GHIElectronics.TinyCLR.UI.Controls.Text(this.fontB, "Will you:")
            {
                ForeColor = Colors.White,
            };

            Canvas.SetLeft(willYouText, 10);
            Canvas.SetTop(willYouText, 10);

            this.canvas.Children.Add(willYouText);


            var checkbox = new CheckBox();

            Canvas.SetTop(checkbox, 50);
            Canvas.SetLeft(checkbox, 50);

            this.canvas.Children.Add(checkbox);

            var backtousText = new GHIElectronics.TinyCLR.UI.Controls.Text(this.fontB, "Back to us.")
            {
                ForeColor = Colors.White,
            };

            Canvas.SetLeft(backtousText, 50 + checkbox.Width + 5);
            Canvas.SetTop(backtousText, 50);

            this.canvas.Children.Add(backtousText);

            var radioButton = new RadioButton()
            {
                Name = "radio1",

            };

            Canvas.SetTop(radioButton, 100);
            Canvas.SetLeft(radioButton, 50);

            this.canvas.Children.Add(radioButton);

            var sellyourverhicleText = new GHIElectronics.TinyCLR.UI.Controls.Text(this.fontB, "Sell your vehicle.")
            {
                ForeColor = Colors.White,
            };

            Canvas.SetLeft(sellyourverhicleText, 50 + radioButton.Width + 5);
            Canvas.SetTop(sellyourverhicleText, 100);

            this.canvas.Children.Add(sellyourverhicleText);

            var radioButton2 = new RadioButton()
            {
                Name = "radio2",

            };

            Canvas.SetTop(radioButton2, 150);
            Canvas.SetLeft(radioButton2, 50);

            this.canvas.Children.Add(radioButton2);

            var donateyourverhicleText = new GHIElectronics.TinyCLR.UI.Controls.Text(this.fontB, "Donate your vehicle.")
            {
                ForeColor = Colors.White,
            };

            Canvas.SetLeft(donateyourverhicleText, 50 + radioButton2.Width + 5);
            Canvas.SetTop(donateyourverhicleText, 150);

            this.canvas.Children.Add(donateyourverhicleText);

            var radioButton3 = new RadioButton()
            {
                Name = "radio3",

            };

            Canvas.SetTop(radioButton3, 200);
            Canvas.SetLeft(radioButton3, 50);

            this.canvas.Children.Add(radioButton3);

            var notsureText = new GHIElectronics.TinyCLR.UI.Controls.Text(this.fontB, "Not sure yet.")
            {
                ForeColor = Colors.White,
            };

            Canvas.SetLeft(notsureText, 50 + radioButton3.Width + 5);
            Canvas.SetTop(notsureText, 200);

            this.canvas.Children.Add(notsureText);

            var doneText = new GHIElectronics.TinyCLR.UI.Controls.Text(this.fontB, "Done!")
            {
                ForeColor = Colors.Black,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,

            };

            var doneButton = new Button()
            {
                Child = doneText,
                Width = 100,
                Height = 40,
            };

            Canvas.SetLeft(doneButton, 370);
            Canvas.SetTop(doneButton, 220);

            this.canvas.Children.Add(doneButton);

            doneButton.Click += this.DoneButton_Click;

            return this.canvas;
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            Program.WpfWindow.Child = Program.SelectServicePage.Elements;
            Program.WpfWindow.Invalidate();
        }
    }
}
