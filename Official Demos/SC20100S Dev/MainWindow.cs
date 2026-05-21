using System;
using System.Collections;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Threading;

namespace Demos {
    internal sealed class MainWindow : Window {
        private readonly UIElement topBar;
        private readonly StackPanel mainStackPanel;
        private readonly StackPanel[] iconStackPanels;

        private int selectWindowIndex;
        private int selectWindowIndexPrev;

        const int IconColumns = 3;
        const int IconRows = 1;

        private readonly ArrayList applicationWindows = new ArrayList();
        private readonly ArrayList showListLeft = new ArrayList();
        private readonly ArrayList showListRight = new ArrayList();
        private readonly ArrayList showListLeftPrev = new ArrayList();
        private readonly ArrayList showListRightPrev = new ArrayList();

        public MainWindow(int width, int height) : base() {
            this.Width = width;
            this.Height = height;

            this.mainStackPanel = new StackPanel(Orientation.Vertical);
            this.Child = this.mainStackPanel;

            this.Background = new LinearGradientBrush(Colors.Blue, Colors.Teal, 0, 0, width, height);

            var topbar = new TopBar(this.Width, "Demo App", true);
            this.topBar = topbar.Child;

            this.iconStackPanels = new StackPanel[IconRows];
            for (var r = 0; r < IconRows; r++)
                this.iconStackPanels[r] = new StackPanel(Orientation.Horizontal);

            this.mainStackPanel.IsVisibleChanged += this.MainStackPanel_IsVisibleChanged;
            this.mainStackPanel.AddHandler(Buttons.ButtonUpEvent, new RoutedEventHandler(this.OnButtonUp), true);
        }

        private void MainStackPanel_IsVisibleChanged(object sender, PropertyChangedEventArgs e) {
            var isVisible = (bool)e.NewValue;
            if (isVisible)
                Buttons.Focus(this.mainStackPanel);
        }

        private void UpdateScreen() {
            this.mainStackPanel.Children.Clear();
            this.mainStackPanel.Children.Add(this.topBar);

            for (var i = 0; i < this.applicationWindows.Count; i++) {
                var a = (ApplicationWindow)this.applicationWindows[i];
                if (a != null)
                    a.Icon.Select = (a.Id == this.selectWindowIndex);
            }

            this.showListLeftPrev.Clear();
            foreach (var e in this.showListLeft) this.showListLeftPrev.Add(e);
            this.showListLeft.Clear();

            this.showListRightPrev.Clear();
            foreach (var e in this.showListRight) this.showListRightPrev.Add(e);
            this.showListRight.Clear();

            const int r = 0;
            this.iconStackPanels[r].Children.Clear();
            this.iconStackPanels[r].SetMargin(0, (this.Height - this.topBar.Height - ((ApplicationWindow)this.applicationWindows[0]).Icon.Height) / 2, 0, 0);

            var left = IconColumns / 2;
            var right = IconColumns / 2;
            var end = this.applicationWindows.Count - 1;

            for (var i = 0; i < left; i++) {
                if (this.selectWindowIndex > 0)
                    this.showListLeft.Add(this.selectWindowIndex - 1 - i);
                else
                    this.showListLeft.Add(end - i);
            }

            for (var i = 0; i < right; i++) {
                if (this.selectWindowIndex < end)
                    this.showListRight.Add(this.selectWindowIndex + 1 + i);
                else
                    this.showListRight.Add(i);
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

            for (var row = 0; row < IconRows; row++)
                this.mainStackPanel.Children.Add(this.iconStackPanels[row]);

            this.RefreshIconCaches();
            this.Invalidate();
        }

        // Keeps only the icons that are currently visible (selected + side
        // slots + previously-visible icons still needed by the slide-out
        // animation) loaded in RAM; releases everyone else. Each decoded
        // 32x32 RGB565 icon costs ~2 KB of native heap, so on a 12-window
        // demo this drops the resident image set from ~24 KB to ~10 KB.
        private void RefreshIconCaches() {
            for (var i = 0; i < this.applicationWindows.Count; i++) {
                var icon = ((ApplicationWindow)this.applicationWindows[i]).Icon;
                if (this.IsIconVisible(i)) icon.EnsureLoaded();
                else                       icon.Unload();
            }
        }

        private bool IsIconVisible(int index) {
            if (index == this.selectWindowIndex) return true;
            if (index == this.selectWindowIndexPrev) return true;
            if (this.showListLeft.Contains(index)) return true;
            if (this.showListRight.Contains(index)) return true;
            if (this.showListLeftPrev.Contains(index)) return true;
            if (this.showListRightPrev.Contains(index)) return true;
            return false;
        }

        public void RegisterWindow(ApplicationWindow aw) {
            aw.Parent = this;
            aw.Id = this.applicationWindows.Count;
            aw.Icon.Width = this.Width / IconColumns;
            aw.Icon.Height = aw.Icon.Width + (3 * aw.Icon.Font.Height) / 2;

            this.applicationWindows.Add(aw);

            if (this.applicationWindows.Count >= IconColumns)
                this.UpdateScreen();
        }

        private void OnButtonUp(object sender, RoutedEventArgs e) {
            var buttonSource = (ButtonEventArgs)e;

            this.selectWindowIndexPrev = this.selectWindowIndex;

            switch (buttonSource.Button) {
                case HardwareButton.Left:
                    if (this.selectWindowIndex == this.applicationWindows.Count - 1)
                        this.selectWindowIndex = 0;
                    else
                        this.selectWindowIndex++;
                    this.animationStep = -MaxStep;
                    break;

                case HardwareButton.Right:
                    if (this.selectWindowIndex == 0)
                        this.selectWindowIndex = this.applicationWindows.Count - 1;
                    else
                        this.selectWindowIndex--;
                    this.animationStep = MaxStep;
                    break;

                case HardwareButton.Select:
                    this.animationStep = 0;
                    var applicationWindow = (ApplicationWindow)this.applicationWindows[this.selectWindowIndex];
                    var nextWindow = applicationWindow.Open();
                    if (nextWindow != null)
                        this.Child = nextWindow;
                    break;
            }

            Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(1000), _ => { this.UpdateScreen(); return null; }, null);
        }

        public void Open() => this.Child = this.mainStackPanel;

        // --- Carousel animation ---
        // When the selection moves left/right, the previously-selected icon
        // shrinks into the corner and the neighbour scales up to fill the
        // centre slot. animationStep goes ±MaxStep on direction change and
        // decays toward 0 over a few frames driven by the dispatcher timer.

        public static int MaxStep = 5;
        public static bool StartAnimation;

        const int TimerInterval = 50;
        private int animationStep;
        private long lastTick;
        private DispatcherTimer animationTimer;

        // Visual envelope of the previously-selected icon as it slides out
        // and the incoming neighbour as it slides in. Indexed by `scale`
        // (0 = full size at animation start; MaxStep = thumbnail size at end).
        // Tuned for 32x32 source icons; if you change the icon size, scale
        // these by the same ratio (the original 50x50 demo used 53 -> 27).
        private readonly int[] widthDownSteps  = { 32, 30, 28, 25, 22, 18 };
        private readonly int[] heightDownSteps = { 32, 30, 28, 25, 22, 18 };

        private void StartAnimationTimer() {
            if (this.animationStep == 0) {
                StartAnimation = false;
                return;
            }

            StartAnimation = true;

            if (this.animationTimer == null) {
                this.animationTimer = new DispatcherTimer(this.Dispatcher) {
                    Interval = TimeSpan.FromMilliseconds(TimerInterval),
                };
                this.animationTimer.Tick += this.OnAnimationTimer;
            }

            this.lastTick = DateTime.Now.Ticks;
            this.animationTimer.Start();
        }

        private void OnAnimationTimer(object o, EventArgs e) {
            this.animationTimer.Stop();

            // Advance proportionally to wall-clock so a missed/slow frame
            // doesn't stretch the animation out.
            var ms = (DateTime.Now.Ticks - this.lastTick) / 10000;
            this.lastTick = DateTime.Now.Ticks;

            var increment = (int)(ms / TimerInterval);
            if (increment < 1) increment = 1;
            else if (increment > MaxStep) increment = MaxStep;

            if (this.animationStep < 0) this.animationStep += increment;
            else if (this.animationStep > 0) this.animationStep -= increment;

            this.Invalidate();
        }

        public override void OnRender(DrawingContext dc) {
            base.OnRender(dc);

            if (StartAnimation) {
                var scaleOffset = Math.Abs(this.animationStep);

                // Previously-selected icon shrinking down.
                {
                    var icon = ((ApplicationWindow)this.applicationWindows[this.selectWindowIndexPrev]).Icon;
                    var scale = MaxStep - scaleOffset;
                    var w = this.widthDownSteps[scale];
                    var h = this.heightDownSteps[scale];

                    // Start the slide-out from where the centred static icon
                    // actually was, not from the slot's left edge. Icon.cs
                    // centres the selected icon by `(slotWidth - imgWidth)/2`,
                    // so match that offset here.
                    var slotWidth = this.Width / IconColumns;
                    var x = slotWidth + (slotWidth - this.widthDownSteps[0]) / 2;
                    var offsetX = (this.animationStep < 0) ? 7 : 13;
                    if (this.animationStep < 0) x -= scale * offsetX;
                    else                        x += scale * offsetX;

                    var y = this.Height - this.topBar.Height - icon.Height - (icon.Font.Height) / 2 + (scale * 5);
                    dc.Scale9Image(x, y, w, h, icon.bitmapImage, icon.RadiusBorder, icon.RadiusBorder, icon.RadiusBorder, icon.RadiusBorder, 100);
                }

                // Incoming icon scaling up — only the immediate neighbour in
                // the direction of motion.
                if (this.animationStep < 0 && this.showListRightPrev.Count > 0) {
                    var icon = ((ApplicationWindow)this.applicationWindows[(int)this.showListRightPrev[0]]).Icon;
                    var scale = scaleOffset;
                    var w = this.widthDownSteps[scale];
                    var h = this.heightDownSteps[scale];

                    var offsetX = (icon.Width + icon.Width / 4) / 5;
                    var x = (this.Width / IconColumns) + icon.Width + icon.Width / 4 - ((MaxStep - scale) * offsetX);
                    var y = this.topBar.Height + icon.Height - (2 * icon.Font.Height) - ((MaxStep - scale) * 5);
                    dc.Scale9Image(x, y, w, h, icon.bitmapImage, icon.RadiusBorder, icon.RadiusBorder, icon.RadiusBorder, icon.RadiusBorder, 100);
                }
                else if (this.animationStep > 0 && this.showListLeftPrev.Count > 0) {
                    var icon = ((ApplicationWindow)this.applicationWindows[(int)this.showListLeftPrev[0]]).Icon;
                    var scale = scaleOffset;
                    var w = this.widthDownSteps[scale];
                    var h = this.heightDownSteps[scale];

                    var offsetX = (icon.Width / 2 + icon.Width / 4) / 5;
                    var x = icon.Width / 4 + ((MaxStep - scale) * offsetX);
                    var y = this.topBar.Height + icon.Height - (2 * icon.Font.Height) - ((MaxStep - scale) * 5);
                    dc.Scale9Image(x, y, w, h, icon.bitmapImage, icon.RadiusBorder, icon.RadiusBorder, icon.RadiusBorder, icon.RadiusBorder, 100);
                }
            }

            this.StartAnimationTimer();
        }
    }
}
