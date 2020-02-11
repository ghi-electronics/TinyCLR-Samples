// Based on original code from Microsoft NETMF WPF
using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;

namespace TextViewer {
    public sealed class Program : Application {
        public DisplayController disp;

        private Program(DisplayController d) : base(d) => this.disp = d;

        public static void Main() {
            var disp = DisplayController.GetDefault();
            disp.SetConfiguration(new ParallelDisplayControllerSettings {
                // Your display configuration
                // This one is for G120E dev Baords
                Width = 320,
                Height = 240,
                DataFormat = DisplayDataFormat.Rgb565,
                PixelClockRate = 15_000_000,
                PixelPolarity = false,
                DataEnablePolarity = true,
                DataEnableIsFixed = true,
                HorizontalFrontPorch = 51,
                HorizontalBackPorch = 27,
                HorizontalSyncPulseWidth = 41,
                HorizontalSyncPolarity = false,
                VerticalFrontPorch = 16,
                VerticalBackPorch = 8,
                VerticalSyncPulseWidth = 10,
                VerticalSyncPolarity = false,
            });

            disp.Enable();

            var gpioController = GpioController.GetDefault();
            var backlight = gpioController.OpenPin(G120E.GpioPin.P3_17);
            backlight.SetDriveMode(GpioPinDriveMode.Output);
            backlight.Write(GpioPinValue.High);

            var app = new Program(disp);

            // The buttons
            var cogpioControllernt = GpioController.GetDefault();
            var right = gpioController.OpenPin(G120E.GpioPin.P2_22);
            var left = gpioController.OpenPin(G120E.GpioPin.P2_21);
            var select = gpioController.OpenPin(G120E.GpioPin.P2_25);
            var up = gpioController.OpenPin(G120E.GpioPin.P2_10);
            var down = gpioController.OpenPin(G120E.GpioPin.P0_22);
            right.SetDriveMode(GpioPinDriveMode.InputPullUp);
            left.SetDriveMode(GpioPinDriveMode.InputPullUp);
            select.SetDriveMode(GpioPinDriveMode.InputPullUp);
            up.SetDriveMode(GpioPinDriveMode.InputPullUp);
            select.SetDriveMode(GpioPinDriveMode.InputPullUp);
            void set(GpioPin pin, HardwareButton btn) => pin.ValueChanged += (s, e) => app.InputProvider.RaiseButton(btn, e.Edge == GpioPinEdge.RisingEdge, DateTime.UtcNow);
            set(right, HardwareButton.Right);
            set(left, HardwareButton.Left);
            set(select, HardwareButton.Select);
            set(up, HardwareButton.Up);
            set(down, HardwareButton.Down);


            app.Run(new TextViewer(disp, "Hello TinyCLR OS",
                "TinyCLR OS is a modern, managed operating system that brings .NET to embedded devices. It offers garbage collection, threading, and full debugging which allows you to step through code and inspect variables. No costly debugging tools are necessary. Microsoft's Visual Studio is used by millions of developers around the world to develop applications for Linux, Mac, and Windows. Its feature set, built-in source control, auto-completion feature, along with many other features make it a breeze when spending the day developing applications. The TinyCLR OS expansion plugs right into Visual Studio giving developers access to all of its features through a single USB cable plugged into the PC and embedded device. Both the paid and free community editions work with TinyCLR OS with no limitations or restrictions."
                ));
        }
    }

    // This class is a helper class for the scrolling text example below; it derives
    // from the TextFlow class and adds functionality that makes it easier to simply
    // pass in a string of text that contains CRLF pairs. This class breaks up the
    // lines into proper TextRuns for the TextFlow class it is based on
    internal sealed class ScrollerText : TextFlow {
        public ScrollerText(string text, System.Drawing.Font font, Color color)
            : base() {

            var pos = 0;

            // Break the text up if it contains CR/LF pairs
            while ((pos = text.IndexOf("\r\n")) > -1) {
                this.TextRuns.Add(new TextRun(text.Substring(0, pos), font, color));
                this.TextRuns.Add(TextRun.EndOfLine);
                pos += 2;
                text = text.Substring(pos, text.Length - pos);
            }

            this.TextRuns.Add(new TextRun(text, font, color));
        }
    }

    // This is the TextScrollView class; it derives from a Panel and wraps a
    // ScrollViewer object and a ScrollerText object (defined above)
    // With those 2 objects it demonstrates how to create a scrollable text object
    internal sealed class TextScrollViewer : Panel {

        // These private values and public member give easy access to controlling
        // how large the horizontal scroll bar is
        private int _hScrollHeight = 3;
        private int _hScrollWidth = 0;
        private double _hScrollRatio = 1;
        public int HScrollHeight {
            get => this._hScrollHeight;
            set {
                this._hScrollHeight = value;
                this._hScrollWidth = this.Width - this._vScrollWidth;
                this._hScrollRatio = this._hScrollWidth - this._vScrollWidth - this._vScrollWidth;
                this._hScrollRatio /= this.Width;
            }
        }

        // These private values and public member give easy access to controlling
        // how large the vertical scroll bar is
        private int _vScrollWidth = 3;
        private int _vScrollHeight = 0;
        private double _vScrollRatio = 1;
        public int VScrollWidth {
            get => this._vScrollWidth;
            set {
                this._vScrollWidth = value;
                this._vScrollHeight = this.Height - this._hScrollHeight;
                this._vScrollRatio = .18;
            }
        }

        // This private member is a standard Micro Framework Presentation object
        // The important member functions are the ones that control the scrolling
        // The TextScrollViewer class provides easy access to those scrolling
        // functions with the 4 following member functions
        private ScrollViewer _viewer;

        // Scroll one line up
        public void LineUp() => this._viewer.LineUp();

        // Scroll one line down
        public void LineDown() => this._viewer.LineDown();

        // Scroll to the left
        public void LineLeft() => this._viewer.LineLeft();

        // Scroll to the right
        public void LineRight() => this._viewer.LineRight();

        // This public member is the text object that is based on the TextFlow class
        public ScrollerText ScrollText;

        public TextScrollViewer(string text, System.Drawing.Font font, Color color) {

            // Create the ScrollViewer object
            this._viewer = new ScrollViewer {
                Background = new SolidColorBrush(Colors.Black)
            };
            // Create the ScrollText object using the parameters passed into
            // the constructor and then set other important member values
            this.ScrollText = new ScrollerText(text, font, color) {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            // Set the child of the viewer to be the ScrollText object
            this._viewer.Child = this.ScrollText;

            // Hard code a line with and height based on the character 'A'
            this._viewer.LineWidth = 20;// System.Drawing.Graphics.MeasureString("A", font);// font.CharWidth('A');
            this._viewer.LineHeight = font.Height;

            // Set the child of our class to be the ScrollViewer object
            this.Children.Add(this._viewer);
        }

        // Normally the presentation framework would arrange our child objects based on their visible
        // size. Since we are tyring to show scrolling beyond that size we override the ArrangeOverride
        // method to set things up to be larger than the screen
        protected override void ArrangeOverride(int arrangeWidth, int arrangeHeight) {
            base.ArrangeOverride(arrangeWidth, arrangeHeight);

            // Set the ScrollViewer's width and height to allow room for
            // the scroll bars
            this._viewer.Width = arrangeWidth - this._vScrollWidth;
            this._viewer.Height = arrangeHeight - this._hScrollHeight;
            this._viewer.Arrange(0, 0, this._viewer.Width, this._viewer.Height);

            // Set the ScrollText's width and height to be three times as big as the allow area
            this.ScrollText.Width = arrangeWidth * 6;
            this.ScrollText.Height = arrangeHeight * 6;
            this.ScrollText.UpdateLayout();
        }

        // Override the OnRender so we can manually draw the scroll bars
        public override void OnRender(DrawingContext dc) {
            base.OnRender(dc);

            // Make a brush and pen for drawing scroll bars
            var brush = new SolidColorBrush(Color.FromRgb(224, 224, 224));
            var sliderBrush = new SolidColorBrush(Color.FromRgb(64, 64, 64));
            var pen = new Pen(Color.FromRgb(64, 64, 64));

            // Draw the horizontal scroll bar
            var hOffset = (int)(this._viewer.HorizontalOffset * this._hScrollRatio);
            dc.DrawRectangle(brush, pen, 0, this.Height - this._hScrollHeight, this._hScrollWidth, this._hScrollHeight);
            dc.DrawRectangle(sliderBrush, pen, (int)(this._viewer.HorizontalOffset * this._hScrollRatio), this.Height - this._hScrollHeight, this._vScrollWidth, this._hScrollHeight);

            // Draw the vertical scroll bar
            var vOffset = (int)(this._viewer.VerticalOffset * this._vScrollRatio);
            dc.DrawRectangle(brush, pen, this.Width - this._vScrollWidth, 0, this._vScrollWidth, this._vScrollHeight);
            dc.DrawRectangle(sliderBrush, pen, this.Width - this._vScrollWidth, vOffset, this._vScrollWidth, this._hScrollHeight);
        }
    }


    // This class demonstrates scrolling a field of text
    internal sealed class TextViewer : Window {

        // This member is the text scroller helper class defined above
        TextScrollViewer _viewer;
        //DemoType demoType;
        // PresentationWindow demo = null;

        public TextViewer(DisplayController disp, string titleString, string scrollTextString)
            : base() {


            this.Width = disp.ActiveConfiguration.Width;
            this.Height = disp.ActiveConfiguration.Height;
            this.Visibility = Visibility.Visible;
            this.Font = Resource.GetFont(Resource.FontResources.NinaB);

            Buttons.Focus(this);

            //this.demoType = demoType;
            // Create a stack panel for the title and scroll view
            var panel = new StackPanel(Orientation.Vertical);

            this._viewer = new TextScrollViewer(scrollTextString, this.Font, Colors.White) {
                Width = this.Width,
                Height = this.Height - 8,  // make room for the title bar
                HScrollHeight = 3,
                VScrollWidth = 3,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            // Create the title text
            var title = new Text(this.Font, titleString) {
                ForeColor = Colors.Black
            };

            // Add the elements to the stack panel
            panel.Children.Add(title);
            panel.Children.Add(this._viewer);

            // Add the stack panel to this window
            this.Child = panel;
            // Set the background color
            this.Background = new SolidColorBrush(Color.FromRgb(64, 64, 255));
        }

        protected override void OnButtonDown(ButtonEventArgs e) {
            switch (e.Button) {
                case HardwareButton.Select:
                    // Remove this window from the Window Manager
                    //this.Close();
                    break;
                case HardwareButton.Up:
                    this._viewer.LineUp();
                    break;
                case HardwareButton.Down:
                    this._viewer.LineDown();
                    break;
                case HardwareButton.Left:
                    this._viewer.LineLeft();
                    break;
                case HardwareButton.Right:
                    this._viewer.LineRight();
                    break;
            }
        }
    }
}
