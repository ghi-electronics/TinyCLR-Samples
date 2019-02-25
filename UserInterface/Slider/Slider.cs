using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;
using GHIElectronics.TinyCLR.UI.Threading;
using System;
using System.Collections;
//using System.Drawing;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.UI.Controls {
    // This control class is a container for the MenuItem controls
    // It handles which MenuItem is current and moving them back and forth
    internal sealed class Slider : Control {

        // Private members
        private int _currentChild = 0;
        private int _width;
        private int _height;
        private int _itemWidth;
        private int _animationStep;

        // This array holds the MenuItems
        public ArrayList MenuItemList;

        public Slider(int width, int height, int itemWidth) {

            // Width and height are passed in and set to our local members
            this._width = width;
            this._height = height;
            this._itemWidth = itemWidth;
            // Create the MenuItem array
            this.MenuItemList = new ArrayList();
        }

        // This method wraps the ArrayList Add method
        public void AddMenuItem(SliderItem menuItem) => this.MenuItemList.Add(menuItem);

        // This property handles getting and setting the current child
        // MenuItem index
        public int CurrentChild {

            // Simply return the current child index
            get => this._currentChild;

            // Setting the current child also kicks off the animation sequence
            set {
                if (value > this._currentChild)
                    this._animationStep = maxStep;        // Moving right
                else if (value < this._currentChild)
                    this._animationStep = -maxStep;       // Moving left
                else
                    this._animationStep = 0;              // Same child, no movement

                if (value >= this.MenuItemList.Count)    // Handle wrapping around right
                    value = 0;

                if (value < 0)                      // Handle wrapping around left
                    value = this.MenuItemList.Count - 1;

                // Set the child and redraw to start the animation
                if (this._animationStep != 0) {
                    this._currentChild = value;
                    this.Invalidate();
                }
            }
        }

        // Using static and constant members allows us to easily change these
        // in one location and affect the many places that these numbers are needed
        static public int maxStep = 5;      // Number of frames in the animation
        const int xOffsetSeparation = 4;    // Distance between each MenuItem
        const int timerInterval = 50;       // Number of MS between each frame

        // Override the OnRender to do the actual drawing of the menu
        public override void OnRender(DrawingContext dc) {

            // Still call the base class in case this control contains other controls
            // Depending on where those controls are placed this may not be optimal
            base.OnRender(dc);

            // Calculate some initial values for positioning and drawing the MenuItems

            // This is the width of each MenuItem
            var largeX = this._itemWidth;// Resource.GetBitmap(Resource.BitmapResources.Desc_Icon).Width + xOffsetSeparation;

            this.Width = this._width;
            this.Height = this._height;
            // This is the starting x position
            var x = (this._width / 2) - ((largeX * 2) + (largeX / 2));

            // This is the starting y position
            var y = 6;

            // This is the scaling of the current MenuItem
            var scale = 0;

            // This is the scaling offset based on the animation step
            var scaleOffset = System.Math.Abs(this._animationStep);

            // adjust the x based on the animation step
            x += this._animationStep * 5;

            // Iterate through the children limiting them to 2 in front and 2 behind
            // the current child. The places the current MenuItem in the middle of
            // the menu
            for (var i = this._currentChild - 2; i < this._currentChild + 3; i++) {

                // If we are on the current child
                if (i == this._currentChild) {

                    // Scale the current child based on the current animation step value
                    // the current child is getting smaller so take the largest value
                    // (maxStep) and subtract the current scaling offset
                    scale = maxStep - scaleOffset;

                }
                else {

                    // If we are moving left and are drawing the child to the left or we
                    // are moving right and are drawing the child to the right then that
                    // child needs to be growing in size
                    // Else the child is drawn without any scaling
                    if ((this._animationStep < 0 && i == this._currentChild + 1) || (this._animationStep > 0 && i == this._currentChild - 1))
                        scale = scaleOffset;
                    else
                        scale = 0;
                }

                // Variable to point to the current MenuItem we want to draw
                SliderItem menuItem = null;

                // Get the correct MenuItem from the array based on the value of i
                // Because we are looking 2 left and 2 right if the current child
                // is near the beginning or end of the array we have to watch for
                // wrapping around then ends
                if (i < 0)
                    menuItem = (SliderItem)this.MenuItemList[this.MenuItemList.Count + i];
                else if (i > this.MenuItemList.Count - 1)
                    menuItem = (SliderItem)this.MenuItemList[i - this.MenuItemList.Count];
                else
                    menuItem = (SliderItem)this.MenuItemList[i];

                // Have the MenuItem render itself based on the position and scaling calculated
                menuItem.Render(dc, x, y, scale);

                // Increment the x position by the size of the MenuItems
                x += largeX;
            }
            // Draw the current menuItem's text
            if (this._width > 0) {
                // Check window size for displaying instructions
                var step = this.Font.Height;
                var row = 54;
                //if (_width < _height)   // Check for portrait display
                // step = 40;

                // Draw the description of the current MenuItem
                var text = ((SliderItem)this.MenuItemList[this._currentChild]).Description;
                dc.DrawText(ref text, this.Font, Colors.White, 0, row, this._width, step, TextAlignment.Center, TextTrimming.None);
            }

            // Start the animation timer
            // This gets called every time the menu is rendered and it will
            // handle decrementing the _animationStep member and stopping the timer
            // when _animationStep reaches 0
            this.StartAnimationTimer();
        }

        // Private timer member and public accessor function
        private DispatcherTimer _animationTimer;
        private void StartAnimationTimer() {

            // Only start the timer if _animationStep is not 0
            if (this._animationStep != 0) {

                // The first time through we will create the timer
                if (this._animationTimer == null) {
                    this._animationTimer = new DispatcherTimer(this.Dispatcher) {
                        Interval = new TimeSpan(0, 0, 0, 0, timerInterval)
                    };
                    this._animationTimer.Tick += new EventHandler(this.OnAnimationTimer);
                }

                // Keep track of when we started the timer to deal with missing
                // frames because of a slow processor or being in the emulator
                this._lastTick = DateTime.Now.Ticks;

                // Start the timer
                this._animationTimer.Start();
            }
        }

        // Private member to keep track of when the timer was started
        // so we can detect missing frames on slow processors and the
        // emulator
        long _lastTick = 0;

        // Timer method to handle the actual timer ticks
        private void OnAnimationTimer(object o, EventArgs e) {

            // Stop the timer while we process this frame
            this._animationTimer.Stop();

            // Figure out how much time has gone by since the timer was started
            var ms = ((DateTime.Now.Ticks - this._lastTick) / 10000);

            // Set the last tick to now
            this._lastTick = DateTime.Now.Ticks;

            // Figure out how many frames should have been displayed by now
            var increment = (int)(ms / timerInterval);

            // If the timer is being serviced in less time than the minimum
            // then we are ok to just process the frame
            // Else If we have gone beyond the maxStep then just move the frame
            // to that one
            if (increment < 1)
                increment = 1;
            else if (increment > maxStep)
                increment = maxStep;

            // Increment _animationStep based on which direction we are going
            if (this._animationStep < 0)
                this._animationStep += increment;
            else if (this._animationStep > 0)
                this._animationStep -= increment;

            // This will trigger another OnRender and kick the timer off again
            // to take the next step in the animation
            this.Invalidate();
        }

        // Override MeasureOverride if you want to hard code the size of your control
        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
            desiredWidth = this._width;
            desiredHeight = this._height;
        }
    }

    // This control class is the MenuItem that handles drawing and actual menu item
    internal sealed class SliderItem : Control {

        // Private members
        private BitmapImage _imageSmall;   // Small image because you can't stretch and image smaller, only larger
        private BitmapImage _image;        // Larger version so it looks good instead of stretching the small one
        private string _description;  // Description of this MenuItem
        private int[] _widthSteps;    // Array of widths so we save time by pre-calculating them
        private int[] _heightSteps;   // Array of heights so we save time by pre-calculating them
                                      //private int _largeWidth;      // Width of large image
                                      //private int _largeHeight;     // Height of large image

        public SliderItem() {
        }

        // This is the constructor that we want to use so we can get all the pieces in one go
        public SliderItem(BitmapImage rBitmapSmall, BitmapImage rBitmap, string description) {

            // Get the images from the resource manager
            this._imageSmall = rBitmapSmall;// BitmapImage.FromGraphics(System.Drawing.Graphics.FromImage(
                                            //Resource.GetBitmap(rBitmapSmall)));
            this._image = rBitmap;// BitmapImage.FromGraphics(System.Drawing.Graphics.FromImage(
                                  // Resource.GetBitmap(rBitmap)));

            // Set the description
            this._description = description;

            // Create the step arrays for zooming in and out
            this._widthSteps = new int[Slider.maxStep];
            this._heightSteps = new int[Slider.maxStep];

            // Get the difference in size between the large and small images
            var wDiff = this._image.Width - this._imageSmall.Width;
            var hDiff = this._image.Height - this._imageSmall.Height;

            // Pre-calculate the width and height values for scaling the image
            for (var i = 1; i < Slider.maxStep; i++) {
                this._widthSteps[i] = (wDiff / Slider.maxStep) * i;
                this._heightSteps[i] = (hDiff / Slider.maxStep) * i;
            }

            // Set the large width and height based on one of the main icons
            //System.Drawing.Bitmap bmp = Resource.GetBitmap(rLargeSizeBitmap);
            //_largeWidth = _image.Width; //bmp.Width;
            //_largeHeight = _image.Height;// bmp.Height;
        }

        // Public accessor method for the description
        public string Description {
            get => this._description;
            set => this._description = value;
        }

        // We are not overriding the OnRender method of the class, but are simply creating
        // a render method that can be called by the menu container
        public void Render(DrawingContext dc, int x, int y, int scale) {
            //var ii = BitmapImage.FromGraphics(System.Drawing.Graphics.FromImage(_image));
            // Make sure we have all of the proper images
            if (this._image != null && this._imageSmall != null) {

                // If the scale is at maxStep then just use the larger image so it looks nice
                // Else use the scale value to scale from the smaller image to something bigger
                if (scale == Slider.maxStep) {
                    this.Width = this._image.Width;
                    this.Height = this._image.Height;
                    dc.DrawImage(this._image, x, y);
                }
                else {
                    // If the scale is 0 then just draw the small bitmap
                    // Else calculate the difference between the small and large bitmaps
                    // and stretch the small bitmap
                    if (scale == 0) {
                        this.Width = this._imageSmall.Width;
                        this.Height = this._imageSmall.Height;
                        x += ((this._image.Width - this.Width) / 2);
                        y += ((this._image.Height - this.Height) / 2);
                        dc.DrawImage(this._imageSmall, x, y);
                    }
                    else {
                        var wDiff = this._image.Width - this._imageSmall.Width;
                        var hDiff = this._image.Height - this._imageSmall.Height;

                        this.Width = this._imageSmall.Width + this._widthSteps[scale];
                        this.Height = this._imageSmall.Height + this._heightSteps[scale];
                        x += ((this._image.Width - this.Width) / 2);
                        y += ((this._image.Height - this.Height) / 2);
                        dc.StretchImage(x, y, this.Width, this.Height, this._imageSmall, 0, 0, this._imageSmall.Width, this._imageSmall.Height, 255);
                    }
                }
            }
        }
    }
}
