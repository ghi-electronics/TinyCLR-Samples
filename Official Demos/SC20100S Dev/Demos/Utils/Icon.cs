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
        private readonly Font font;

        public bool Select { get; set; }
        public int Id { get; set; }

        public string IconText { get; }

        static GHIElectronics.TinyCLR.UI.Media.Color TextColor { get; set; }

        public Icon(Bitmap icon, string text) : base() {
            if (icon != null)
                this.bitmapImage = BitmapImage.FromGraphics(Graphics.FromImage(icon));

            this.IconText = text;
            this.font = Resources.GetFont(Resources.FontResources.droid_reg08);

            TextColor = Colors.White;
            this.Width = 40;
            this.Height = 40;
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
                dc.Scale9Image(x + w / 4, y + h / 4, w - w / 2, h - h / 2, this.bitmapImage, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, alpha);
            }            

            if (this.IconText != null && this.font != null) {
                var text = this.IconText;

                dc.DrawText(ref text, this.font, TextColor, 0, h - this.font.Height, w, this.font.Height, TextAlignment.Center, TextTrimming.None);
            }
                       
        }
    }
}
