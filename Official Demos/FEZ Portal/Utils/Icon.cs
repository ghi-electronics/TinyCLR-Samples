using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;


namespace Demos {
    public class Icon : Button {
        private readonly BitmapImage bitmapImage;

        public bool Select { get; set; }
        public int Id { get; set; }

        public string IconText { get; }

        static GHIElectronics.TinyCLR.UI.Media.Color TextColor { get; set; }

        public Icon(Bitmap icon, string text) : base() {
            if (icon != null) {
                var gfx = Graphics.FromImage(icon);

                gfx.MakeTransparent(System.Drawing.Color.FromArgb(0x00FF00F2)); // MakeTransparent is only available from TinyCLR OS rc2

                this.bitmapImage = BitmapImage.FromGraphics(gfx);
            }

            this.IconText = text;
            this.Font = Resources.GetFont(Resources.FontResources.droid_reg09);

            TextColor = Colors.White;
        }

        public override void OnRender(DrawingContext dc) {
            var alpha = (this.IsEnabled) ? this.Alpha : (ushort)(this.Alpha / 2);

            var x = 0;
            var y = 0;

            var w = this.Width;
            var h = this.Height;

            if (this.Select) {
                dc.DrawRectangle(new SolidColorBrush(GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0, 0, 0xFF)), new GHIElectronics.TinyCLR.UI.Media.Pen(GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0, 0xFF, 0xFF)), 0, 0, w, h);
            }

            if (this.bitmapImage != null) {
                dc.DrawImage(this.bitmapImage, x + w / 4, y + h / 4);
            }

            if (this.IconText != null && this.Font != null) {
                var text = this.IconText;

                dc.DrawText(ref text, this.Font, TextColor, 0, h - this.Font.Height, w, this.Font.Height, TextAlignment.Center, TextTrimming.None);
            }

        }
    }
}
