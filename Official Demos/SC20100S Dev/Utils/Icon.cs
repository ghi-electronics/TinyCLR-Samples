using Demos.Properties;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;
using SystemDrawing = System.Drawing;

namespace Demos {
    internal class Icon : Button {
        // Lazy-loaded: the BitmapImage is allocated only when the carousel
        // actually needs to draw this icon, and released when it scrolls back
        // off-screen. This keeps at most ~3 icons resident in RAM at a time
        // (selected + previous neighbours for the slide-out animation),
        // instead of all 12. Critical for the SC20100 because each decoded
        // 32x32 RGB565 bitmap is ~2 KB of native heap.
        private BitmapImage cachedImage;
        private readonly Resources.BitmapResources resource;

        public bool Select { get; set; }
        public int Id { get; set; }
        public string IconText { get; }

        private readonly Color textColor = Colors.White;

        public Icon(Resources.BitmapResources resource, string text) : base() {
            this.resource = resource;
            this.IconText = text;
            this.Font = Resources.GetFont(Resources.FontResources.droid_reg08);
        }

        // BitmapImage accessor for MainWindow.OnRender — kept here (rather
        // than as a public property of a different name) to minimize the
        // carousel-render diff. May be null until EnsureLoaded is called.
        public BitmapImage bitmapImage => this.cachedImage;

        public void EnsureLoaded() {
            if (this.cachedImage != null) return;

            var bitmap = Resources.GetBitmap(this.resource);
            if (bitmap == null) return;

            var gfx = SystemDrawing.Graphics.FromImage(bitmap);
            this.cachedImage = BitmapImage.FromGraphics(gfx);
        }

        public void Unload() {
            // Dropping the reference lets the GC reclaim the BitmapImage and
            // its underlying Graphics buffer on the next collection. The UI
            // framework keeps the buffer field internal so we can't call
            // Dispose directly — but for our purposes (free RAM as the
            // carousel scrolls) GC is sufficient.
            this.cachedImage = null;
        }

        public override void OnRender(DrawingContext dc) {
            var alpha = (this.IsEnabled) ? this.Alpha : (ushort)(this.Alpha / 2);

            var w = this.Width;
            var h = this.Height - (3 * this.Font.Height) / 2;

            if (MainWindow.StartAnimation)
                return;

            this.EnsureLoaded();

            if (this.Select) {
                if (this.cachedImage != null) {
                    // Centre the image inside the slot. The slot is sized to
                    // `Width x Height` (here w x h after the text strip), but
                    // the icon bitmap is smaller — left-aligning at (0,0)
                    // makes the middle icon look anchored to the corner.
                    var x = (w - this.cachedImage.Width) / 2;
                    var y = (h - this.cachedImage.Height) / 2;
                    dc.DrawImage(this.cachedImage, x, y);
                }

                if (this.IconText != null && this.Font != null) {
                    var text = this.IconText;
                    dc.DrawText(ref text, this.Font, this.textColor, 0, h, w, this.Font.Height, TextAlignment.Center, TextTrimming.None);
                }
            }
            else if (this.cachedImage != null) {
                dc.Scale9Image(w / 4, h - h / 2, w / 2, h / 2, this.cachedImage, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, alpha);
            }
        }
    }
}
