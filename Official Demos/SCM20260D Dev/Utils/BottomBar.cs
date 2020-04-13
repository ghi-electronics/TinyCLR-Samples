using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;

namespace Demos.Utils {
    public class BottomBar {
        private Canvas canvas;

        public Button ButtonBack { get; }
        public Button ButtonNext { get; }

        private Font font;

        private int buttonWidth;
        private int buttonHeight;

        public UIElement Child { get; private set; }

        public int Width { get; set; }
        public int Height { get; set; }

        private readonly bool enableButtonBack;
        private readonly bool enableButtonNext;

        public BottomBar(int width, bool enableButtonBack, bool enableButtonNext) {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg12);
            this.buttonWidth = 80;
            this.buttonHeight = this.font.Height + 8;
            this.enableButtonBack = enableButtonBack;
            this.enableButtonNext = enableButtonNext;

            this.Width = width;
            this.Height = this.buttonHeight;

            this.canvas = new Canvas {
                Width = this.Width,
                Height = this.buttonHeight
            };

            if (this.enableButtonBack) {
                var backText = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, "Back") {
                    ForeColor = Colors.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                this.ButtonBack = new Button() {
                    Child = backText,
                    Width = this.buttonWidth,
                    Height = this.buttonHeight,
                };

                Canvas.SetLeft(this.ButtonBack, 0);
                Canvas.SetTop(this.ButtonBack, 0);

                this.canvas.Children.Add(this.ButtonBack);


            }

            if (this.enableButtonNext) {
                var nextText = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, "Next") {
                    ForeColor = Colors.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                this.ButtonNext = new Button() {
                    Child = nextText,
                    Width = this.buttonWidth,
                    Height = this.buttonHeight,
                };

                Canvas.SetRight(this.ButtonNext, 0);
                Canvas.SetTop(this.ButtonNext, 0);

                this.canvas.Children.Add(this.ButtonNext);

            }

            this.Child = this.canvas;
        }

        public void Dispose() {
            this.canvas.Children.Clear();
            if (this.enableButtonBack && this.ButtonBack != null)
                this.ButtonBack.Dispose();

            if (this.enableButtonNext && this.ButtonNext != null)
                this.ButtonNext.Dispose();

            this.font.Dispose();
        }
    }
}
