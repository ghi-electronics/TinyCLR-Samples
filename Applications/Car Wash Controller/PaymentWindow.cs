using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using CarWashExample.Properties;

namespace CarWashExample {
    public sealed class PaymentWindow {
        private Canvas canvas;
        private Font font;
        private Font fontB;        

        public UIElement Elements { get; }

        public PaymentWindow() {
            this.canvas = new Canvas();
            this.font = Resources.GetFont(Resources.FontResources.small);
            this.fontB = Resources.GetFont(Resources.FontResources.NinaB);
            OnScreenKeyboard.Font = this.fontB;


            this.Elements = this.CreatePage();

        }    

        private UIElement CreatePage() {
            this.canvas.Children.Clear();

            var creditCardText = new GHIElectronics.TinyCLR.UI.Controls.Text(this.fontB, "Input your credit card number :") {
                ForeColor = Colors.White,
            };

            Canvas.SetLeft(creditCardText, 10);
            Canvas.SetTop(creditCardText, 20);

            this.canvas.Children.Add(creditCardText);

            var expireText = new GHIElectronics.TinyCLR.UI.Controls.Text(this.fontB, "Expire date :") {
                ForeColor = Colors.White,
            };

            Canvas.SetLeft(expireText, 132);
            Canvas.SetTop(expireText, 50);

            this.canvas.Children.Add(expireText);

            var pinTex = new GHIElectronics.TinyCLR.UI.Controls.Text(this.fontB, "Pin :") {
                ForeColor = Colors.White,
            };

            Canvas.SetLeft(pinTex, 187);
            Canvas.SetTop(pinTex, 80);

            this.canvas.Children.Add(pinTex);


            var creditCardTextBox = new TextBox() {
                Text = "#########",
                Font = fontB,
                Width = 120,
                Height = 25,

            };

            Canvas.SetLeft(creditCardTextBox, 250);
            Canvas.SetTop(creditCardTextBox, 15);

            this.canvas.Children.Add(creditCardTextBox);

            var exprireTexBox = new TextBox() {
                Text = "01/01/2020",
                Font = fontB,
                Width = 120,
                Height = 25,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,

            };

            Canvas.SetLeft(exprireTexBox, 250);
            Canvas.SetTop(exprireTexBox, 45);

            this.canvas.Children.Add(exprireTexBox);

            var pinTexBox = new TextBox() {
                Text = "0000",
                Font = fontB,
                Width = 120,
                Height = 25,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,

            };

            Canvas.SetLeft(pinTexBox, 250);
            Canvas.SetTop(pinTexBox, 75);

            this.canvas.Children.Add(pinTexBox);

            var backButton = new Button() {
                Child = new GHIElectronics.TinyCLR.UI.Controls.Text(this.fontB, "Back") {
                    ForeColor = Colors.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                Width = 100,
                Height = 40,
            };

            var goButton = new Button() {
                Child = new GHIElectronics.TinyCLR.UI.Controls.Text(this.fontB, "Next") {
                    ForeColor = Colors.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                Width = 100,
                Height = 40,
            };

            Canvas.SetLeft(backButton, 10);
            Canvas.SetTop(backButton, 220);

            this.canvas.Children.Add(backButton);

            Canvas.SetLeft(goButton, 370);
            Canvas.SetTop(goButton, 220);

            this.canvas.Children.Add(goButton);

            backButton.Click += this.BackButton_Click;
            goButton.Click += this.GoButton_Click;

            return this.canvas;
        }



        private void GoButton_Click(object sender, RoutedEventArgs e) {
            if (e.RoutedEvent.Name.CompareTo("TouchUpEvent") == 0) {               

                var msgBox = new MessageBox(this.fontB);

                msgBox.Show("Are you sure?", "Confirm", MessageBox.MessageBoxButtons.YesNo);

                msgBox.ButtonClick += (a, b) => {

                    if (b.DialogResult == MessageBox.DialogResult.Yes) {
                        Program.WpfWindow.Child = Program.LoadingPage.Elements;
                        Program.LoadingPage.Active();
                    }

                };

                Program.WpfWindow.Invalidate();
            }

        }

        private void BackButton_Click(object sender, RoutedEventArgs e) {    
            Program.WpfWindow.Child = Program.SelectServicePage.Elements;
            Program.WpfWindow.Invalidate();
        }
    }
}
