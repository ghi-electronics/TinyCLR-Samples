using System;
using System.Drawing;
using System.Threading;
using Demos.Properties;
using Demos.Utils;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;

namespace Demos {
    internal class ApplicationWindow {
        public int Id { get => this.Icon.Id; set => this.Icon.Id = value; }
        public Icon Icon { get; set; }
        public MainWindow Parent { get; set; }

        public bool EnableClockOnTopBar { get; set; }
        public bool EnableButtonBack { get; set; }
        public bool EnableButtonNext { get; set; }

        private TopBar topBar;
        private BottomBar bottomBar;

        protected UIElement Child { get; set; }
        protected int Width { get; set; }
        protected int Height { get; set; }

        public delegate void OnBottomBarButtonUpEventHandle(object sender, RoutedEventArgs e);
        public event OnBottomBarButtonUpEventHandle OnBottomBarButtonUpEvent;

        public delegate void OnBottomBarTouchUpEventHandle(object sender, RoutedEventArgs e);
        public event OnBottomBarTouchUpEventHandle OnBottomBarButtonBackTouchUpEvent;
        public event OnBottomBarTouchUpEventHandle OnBottomBarButtonNextTouchUpEvent;

        private bool active;

        public ApplicationWindow(Resources.BitmapResources iconResource, string title, int width, int height) {
            this.Icon = new Icon(iconResource, title);
            this.Width = width;
            this.Height = height;
        }

        public UIElement TopBar => this.topBar?.Child;
        public UIElement BottomBar => this.bottomBar?.Child;

        protected virtual void Active() { }
        protected virtual void Deactive() { }

        public UIElement Open() {
            if (this.active)
                return this.Child;

            this.topBar = new TopBar(this.Width, this.Icon.IconText, this.EnableClockOnTopBar);

            if (this.EnableButtonBack || this.EnableButtonNext)
                this.bottomBar = new BottomBar(this.Width, this.EnableButtonBack, this.EnableButtonNext);

            try {
                this.Active();
            }
            catch {
                return null;
            }

            if (this.EnableButtonBack || this.EnableButtonNext) {
                this.Child.AddHandler(Buttons.ButtonUpEvent, new RoutedEventHandler(this.OnButtonUp), true);
                this.Child.IsVisibleChanged += this.Child_IsVisibleChanged;

                if (this.EnableButtonBack)
                    this.bottomBar.ButtonBack.Click += this.ButtonBack_Click;

                if (this.EnableButtonNext)
                    this.bottomBar.ButtonNext.Click += this.ButtonNext_Click;
            }

            this.active = true;
            return this.Child;
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e) =>
            this.OnBottomBarButtonBackTouchUpEvent?.Invoke(sender, e);

        private void ButtonNext_Click(object sender, RoutedEventArgs e) =>
            this.OnBottomBarButtonNextTouchUpEvent?.Invoke(sender, e);

        private void Child_IsVisibleChanged(object sender, PropertyChangedEventArgs e) {
            var isVisible = (bool)e.NewValue;

            if (this.Child != null && isVisible)
                Buttons.Focus(this.Child);
        }

        public void Close() {
            if (this.active) {
                this.Deactive();

                this.topBar?.Dispose();
                this.bottomBar?.Dispose();

                if (this.bottomBar != null) {
                    if (this.EnableButtonBack)
                        this.bottomBar.ButtonBack.Click -= this.ButtonBack_Click;
                    if (this.EnableButtonNext)
                        this.bottomBar.ButtonNext.Click -= this.ButtonNext_Click;
                }

                if (this.Child != null)
                    this.Child.IsVisibleChanged -= this.Child_IsVisibleChanged;

                this.topBar = null;
                this.bottomBar = null;
                this.active = false;
            }

            this.Parent?.Open();
        }

        private void OnButtonUp(object sender, RoutedEventArgs e) =>
            this.OnBottomBarButtonUpEvent?.Invoke(sender, e);

        public void UpdateStatusText(TextFlow textFlow, string text, Font font, bool clearScreen) =>
            this.UpdateStatusText(textFlow, text, font, clearScreen, Color.White);

        public void UpdateStatusText(TextFlow textFlow, string text, Font font, bool clearScreen, Color color) {
            if (textFlow == null) return;

            int expectedCount;
            lock (textFlow) {
                expectedCount = textFlow.TextRuns.Count + 2;
            }

            Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(100), _ => {
                if (textFlow == null) return null;
                lock (textFlow) {
                    if (clearScreen)
                        textFlow.TextRuns.Clear();

                    textFlow.TextRuns.Add(text, font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(color.R, color.G, color.B));
                    textFlow.TextRuns.Add(TextRun.EndOfLine);
                    return null;
                }
            }, null);

            // Wait for the dispatched mutation to land so the caller can chain
            // updates without races against TextRuns.Count.
            lock (textFlow) {
                var target = clearScreen ? 2 : expectedCount;
                while (textFlow.TextRuns.Count < target) {
                    Thread.Sleep(1);
                }
            }
        }
    }
}
