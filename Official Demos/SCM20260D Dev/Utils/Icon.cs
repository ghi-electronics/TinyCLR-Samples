using Demos.Properties;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;
using SystemDrawing = System.Drawing;

namespace Demos {
    public class Icon : Button {
        private readonly BitmapImage bitmapImage;
        private readonly Color textColor = Colors.White;

        public bool Select { get; set; }
        public int Id { get; set; }
        public string IconText { get; }

        public Icon(SystemDrawing.Bitmap icon, string text) : base() {
            if (icon != null) {
                var gfx = SystemDrawing.Graphics.FromImage(icon);
                gfx.MakeTransparent(SystemDrawing.Color.FromArgb(0x00FF00F2));
                this.bitmapImage = BitmapImage.FromGraphics(gfx);
            }

            this.IconText = text;
            this.Font = Resources.GetFont(Resources.FontResources.droid_reg09);
        }

        public override void OnRender(DrawingContext dc) {
            var w = this.Width;
            var h = this.Height;

            if (this.Select) {
                dc.DrawRectangle(
                    new SolidColorBrush(Color.FromRgb(0, 0, 0xFF)),
                    new Pen(Color.FromRgb(0, 0xFF, 0xFF)),
                    0, 0, w, h);
            }

            if (this.bitmapImage != null) {
                dc.DrawImage(this.bitmapImage, w / 4, h / 4);
            }

            if (this.IconText != null && this.Font != null) {
                var text = this.IconText;
                dc.DrawText(ref text, this.Font, this.textColor, 0, h - this.Font.Height, w, this.Font.Height,
                    TextAlignment.Center, TextTrimming.None);
            }
        }
    }
}
