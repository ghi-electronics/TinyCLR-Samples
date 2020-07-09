using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;
using GHIElectronics.TinyCLR.UI.Shapes;
using GHIElectronics.TinyCLR.UI.Threading;

namespace Demos {

    public sealed class MainWindow : Window {

        private UIElement topBar;
        private readonly StackPanel mainStackPanel;
        private StackPanel[] iconStackPanels;

        private int selectWindowIndex = 0;
        private int selectWindowIndexPrev = 0;

        const int IconColum = 3;
        const int IconRow = 1;

        private ArrayList applicationWindows;
        private ArrayList showListLeft;
        private ArrayList showListRight;

        private ArrayList showListLeftPrev;
        private ArrayList showListRightPrev;

        public MainWindow(int width, int height) : base() {
            this.Width = width;
            this.Height = height;

            this.mainStackPanel = new StackPanel(Orientation.Vertical);
            this.Child = this.mainStackPanel;

            this.Background = new LinearGradientBrush(Colors.Blue, Colors.Teal, 0, 0, width, height);

            this.CreateTopBar();
            this.CreateIcons();

            this.mainStackPanel.IsVisibleChanged += this.MainStackPanel_IsVisibleChanged;
            this.mainStackPanel.AddHandler(Buttons.ButtonUpEvent, new RoutedEventHandler(this.OnButtonUp), true);

            this.applicationWindows = new ArrayList();
            this.showListLeft = new ArrayList();
            this.showListRight = new ArrayList();

            this.showListLeftPrev = new ArrayList();
            this.showListRightPrev = new ArrayList();
        }

        private void MainStackPanel_IsVisibleChanged(object sender, PropertyChangedEventArgs e) {
            var isVisible = (bool)e.NewValue;

            if (isVisible)
                Buttons.Focus(this.mainStackPanel);
        }

        private void CreateTopBar() {
            var topbar = new TopBar(this.Width, "Demo App", true);
            this.topBar = topbar.Child;
        }

        private void CreateIcons() {
            this.iconStackPanels = new StackPanel[IconRow];

            for (var r = 0; r < IconRow; r++) {
                this.iconStackPanels[r] = new StackPanel(Orientation.Horizontal);
            }
        }

        private void UpdateScreen() {
            this.mainStackPanel.Children.Clear();
            this.mainStackPanel.Children.Add(this.topBar);

            for (var i = 0; i < this.applicationWindows.Count; i++) {
                var a = (ApplicationWindow)this.applicationWindows[i];
                if (a != null) {
                    if (a.Id == this.selectWindowIndex) {
                        a.Icon.Select = true;
                    }
                    else {
                        a.Icon.Select = false;
                    }
                }
            }

            var r = 0;

            this.showListLeftPrev.Clear();

            foreach (var e in this.showListLeft) {
                this.showListLeftPrev.Add(e);
            }

            this.showListLeft.Clear();

            this.iconStackPanels[r].Children.Clear();
            this.iconStackPanels[r].SetMargin(0, (this.Height - this.topBar.Height - ((ApplicationWindow)this.applicationWindows[0]).Icon.Height) / 2, 0, 0);

            var left = IconColum / 2;
            var right = IconColum / 2;

            var end = this.applicationWindows.Count - 1;

            for (var i = 0; i < left; i++) {
                if (this.selectWindowIndex > 0) {
                    this.showListLeft.Add(this.selectWindowIndex - 1 - i);

                }
                else {
                    this.showListLeft.Add(end - i);
                }
            }

            this.showListRightPrev.Clear();

            foreach (var e in this.showListRight) {
                this.showListRightPrev.Add(e);
            }

            this.showListRight.Clear();

            for (var i = 0; i < right; i++) {
                if (this.selectWindowIndex < end) {
                    this.showListRight.Add(this.selectWindowIndex + 1 + i);

                }
                else {
                    this.showListRight.Add(i);
                }
            }


            for (var i = left - 1; i >= 0; i--) {
                var a = (ApplicationWindow)this.applicationWindows[(int)this.showListLeft[i]];

                this.iconStackPanels[r].Children.Add(a.Icon);
            }

            this.iconStackPanels[r].Children.Add(((ApplicationWindow)this.applicationWindows[this.selectWindowIndex]).Icon);

            for (var i = 0; i < right; i++) {
                var a = (ApplicationWindow)this.applicationWindows[(int)this.showListRight[i]];

                this.iconStackPanels[r].Children.Add(a.Icon);
            }


            for (r = 0; r < IconRow; r++) {
                this.mainStackPanel.Children.Add(this.iconStackPanels[r]);
            }

            this.Invalidate();
        }

        public void RegisterWindow(ApplicationWindow aw) {

            GC.Collect();
            GC.WaitForPendingFinalizers();


            aw.Parent = this;
            aw.Id = this.applicationWindows.Count;
            aw.Icon.Width = this.Width / IconColum;
            aw.Icon.Height = aw.Icon.Width + (3 * aw.Icon.Font.Height) / 2;

            this.applicationWindows.Add(aw);


            if (this.applicationWindows.Count >= IconColum)
                this.UpdateScreen();

        }

        private void OnButtonUp(object sender, RoutedEventArgs e) {
            var buttonSource = (GHIElectronics.TinyCLR.UI.Input.ButtonEventArgs)e;

            this.selectWindowIndexPrev = this.selectWindowIndex;

            switch (buttonSource.Button) {
                case GHIElectronics.TinyCLR.UI.Input.HardwareButton.Left:
                    if (this.selectWindowIndex == this.applicationWindows.Count - 1) {
                        this.selectWindowIndex = 0;
                    }
                    else {
                        this.selectWindowIndex++;
                    }

                    this.animationStep = -MaxStep;


                    break;

                case GHIElectronics.TinyCLR.UI.Input.HardwareButton.Right:
                    if (this.selectWindowIndex == 0) {
                        this.selectWindowIndex = this.applicationWindows.Count - 1;
                    }
                    else {
                        this.selectWindowIndex--;
                    }

                    this.animationStep = MaxStep;

                    break;

                case GHIElectronics.TinyCLR.UI.Input.HardwareButton.Select:
                    this.animationStep = 0;

                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    var applicationWindow = (ApplicationWindow)this.applicationWindows[this.selectWindowIndex];

                    var nextWindow = applicationWindow.Open();

                    if (nextWindow != null)
                        this.Child = nextWindow;

                    break;

            }

            Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(1000), _ => { this.UpdateScreen(); return null; }, null);

        }

        public void Open() {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            this.Child = this.mainStackPanel;
        }

        public static bool DrawIcon;

        private DispatcherTimer animationTimer;

        static public int MaxStep = 5;      // Number of frames in the animation

        const int TimerInterval = 50;       // Number of MS between each frame
        private int animationStep;
        private long lastTick;

        private int[] widthDownSteps = new int[] { 53, 48, 43, 38, 33, 27 };    // Array of widths so we save time by pre-calculating them
        private int[] heightDownSteps = new int[] { 53, 48, 43, 38, 33, 27 };

        public static bool StartAnimation = false;

        private void StartAnimationTimer() {

            // Only start the timer if _animationStep is not 0
            if (this.animationStep != 0) {
                StartAnimation = true;
                // The first time through we will create the timer
                if (this.animationTimer == null) {
                    this.animationTimer = new DispatcherTimer(this.Dispatcher) {
                        Interval = new TimeSpan(0, 0, 0, 0, TimerInterval)
                    };
                    this.animationTimer.Tick += this.OnAnimationTimer;
                }

                // Keep track of when we started the timer to deal with missing
                // frames because of a slow processor or being in the emulator
                this.lastTick = DateTime.Now.Ticks;

                // Start the timer
                this.animationTimer.Start();
            }
            else {
                StartAnimation = false;
            }
        }

        private void OnAnimationTimer(object o, EventArgs e) {

            // Stop the timer while we process this frame
            this.animationTimer.Stop();

            // Figure out how much time has gone by since the timer was started
            var ms = ((DateTime.Now.Ticks - this.lastTick) / 10000);

            // Set the last tick to now
            this.lastTick = DateTime.Now.Ticks;

            // Figure out how many frames should have been displayed by now
            var increment = (int)(ms / TimerInterval);

            // If the timer is being serviced in less time than the minimum
            // then we are ok to just process the frame
            // Else If we have gone beyond the maxStep then just move the frame
            // to that one
            if (increment < 1)
                increment = 1;
            else if (increment > MaxStep)
                increment = MaxStep;

            // Increment _animationStep based on which direction we are going
            if (this.animationStep < 0)
                this.animationStep += increment;
            else if (this.animationStep > 0)
                this.animationStep -= increment;

            // This will trigger another OnRender and kick the timer off again
            // to take the next step in the animation
            this.Invalidate();
        }

        public static bool LockIcon = false;

        public override void OnRender(DrawingContext dc) {
            base.OnRender(dc);

            if (StartAnimation == true) {

                var scaleOffset = System.Math.Abs(this.animationStep);

                {
                    var a = (ApplicationWindow)this.applicationWindows[this.selectWindowIndexPrev];
                    var icon = a.Icon;

                    var scale = MaxStep - scaleOffset;

                    var w = this.widthDownSteps[scale];
                    var h = this.heightDownSteps[scale];

                    var x = this.Width / IconColum;//  startX + this.animationStep * 5;


                    var offsetY = 5;

                    if (this.animationStep < 0) {
                        var offsetX = 7;
                        x -= (scale * offsetX);
                    }
                    else {
                        var offsetX = 13;
                        x += (scale * offsetX);
                    }

                    var y = this.Height - this.topBar.Height - icon.Height - (icon.Font.Height) / 2 + (scale * offsetY);

                    dc.Scale9Image(x, y, w, h, icon.bitmapImage, icon.RadiusBorder, icon.RadiusBorder, icon.RadiusBorder, icon.RadiusBorder, 100);

                }


                if (this.animationStep < 0) {
                    if (this.showListRightPrev.Count > 0) {
                        var i = 0; // draw the one next to Seclect item only

                        var a = (ApplicationWindow)this.applicationWindows[(int)this.showListRightPrev[i]];

                        var icon = a.Icon;

                        var scale = scaleOffset;

                        var w = this.widthDownSteps[scale];
                        var h = this.heightDownSteps[scale];

                        var offsetX = (icon.Width + icon.Width / 4) / 5;

                        var x = (this.Width / IconColum) + icon.Width + icon.Width / 4 - ((MaxStep - scale) * offsetX);

                        var offsetY = 5;

                        var y = this.topBar.Height + icon.Height - (2 * icon.Font.Height) - ((MaxStep - scale) * offsetY);

                        dc.Scale9Image(x, y, w, h, icon.bitmapImage, icon.RadiusBorder, icon.RadiusBorder, icon.RadiusBorder, icon.RadiusBorder, 100);

                    }


                }

                if (this.animationStep > 0) {
                    if (this.showListLeftPrev.Count > 0) {
                        var i = 0; // draw the one next to Seclect item only

                        var a = (ApplicationWindow)this.applicationWindows[(int)this.showListLeftPrev[i]];

                        var icon = a.Icon;

                        var scale = scaleOffset;

                        var w = this.widthDownSteps[scale];
                        var h = this.heightDownSteps[scale];

                        var offsetX = (icon.Width / 2 + icon.Width / 4) / 5;

                        var x = icon.Width / 4 + ((MaxStep - scale) * offsetX);

                        var offsetY = 5;

                        var y = this.topBar.Height + icon.Height - (2 * icon.Font.Height) - ((MaxStep - scale) * offsetY);

                        dc.Scale9Image(x, y, w, h, icon.bitmapImage, icon.RadiusBorder, icon.RadiusBorder, icon.RadiusBorder, icon.RadiusBorder, 100);
                    }

                }
            }

            this.StartAnimationTimer();
        }
    }
}
