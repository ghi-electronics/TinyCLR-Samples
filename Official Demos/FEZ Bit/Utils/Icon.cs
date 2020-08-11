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
        public BitmapImage bitmapImage;

        public bool Select { get; set; }
        public int Id { get; set; }

        public string IconText { get; }

        static GHIElectronics.TinyCLR.UI.Media.Color TextColor { get; set; }

        public Icon(Bitmap icon, string text) : base() {
            if (icon != null) {
                var gfx = Graphics.FromImage(icon);

                //gfx.MakeTransparent(0xFFFFFF);  // MakeTransparent is only available from TinyCLR OS rc2

                this.bitmapImage = BitmapImage.FromGraphics(gfx);
            }

            this.IconText = text;
            this.Font = Resources.GetFont(Resources.FontResources.droid_reg08);

            TextColor = Colors.White;

        }

        public override void OnRender(DrawingContext dc) {
            var alpha = (this.IsEnabled) ? this.Alpha : (ushort)(this.Alpha / 2);

            var x = 0;
            var y = 0;

            var w = this.Width;
            var h = this.Height - (3 * this.Font.Height) / 2;

            if (MainWindow.StartAnimation)
                return;

            if (this.Select) {

                if (this.bitmapImage != null) {
                    dc.DrawImage(this.bitmapImage, x,y);
                }

                if (this.IconText != null && this.Font != null) {
                    var text = this.IconText;

                    dc.DrawText(ref text, this.Font, TextColor, 0, h, w, this.Font.Height, TextAlignment.Center, TextTrimming.None);
                }
            }

            else {

                if (this.bitmapImage != null) {
                    dc.Scale9Image(x + w / 4, y + (h - h / 2), w / 2, h / 2, this.bitmapImage, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, alpha);
                }


            }
        }
    }
}
