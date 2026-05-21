using System;
using System.Collections;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;

namespace Demos {
    public sealed class MainWindow : Window {
        private UIElement topBar;
        private readonly StackPanel mainStackPanel;
        private StackPanel[] iconStackPanels;

        const int IconColumns = 6;
        const int IconRows = 3;
        const int MaxWindows = IconColumns * IconRows;

        private readonly ArrayList applicationWindows;

        public MainWindow(int width, int height) : base() {
            this.Width = width;
            this.Height = height;

            this.mainStackPanel = new StackPanel(Orientation.Vertical);
            this.Child = this.mainStackPanel;

            this.Background = new LinearGradientBrush(Colors.Blue, Colors.Teal, 0, 0, width, height);

            this.CreateTopBar();
            this.CreateIcons();

            this.applicationWindows = new ArrayList();
        }

        private void CreateTopBar() {
            var topbar = new TopBar(this.Width, "Demo App", true);
            this.topBar = topbar.Child;
        }

        private void CreateIcons() {
            this.iconStackPanels = new StackPanel[IconRows];

            for (var r = 0; r < IconRows; r++) {
                this.iconStackPanels[r] = new StackPanel(Orientation.Horizontal);
            }
        }

        private void UpdateScreen() {
            this.mainStackPanel.Children.Clear();
            this.mainStackPanel.Children.Add(this.topBar);

            for (var r = 0; r < IconRows; r++) {
                this.mainStackPanel.Children.Add(this.iconStackPanels[r]);
            }

            this.Invalidate();
        }

        public void RegisterWindow(ApplicationWindow aw) {
            if (this.applicationWindows.Count == MaxWindows)
                throw new ArgumentOutOfRangeException(
                    nameof(aw),
                    "RegisterWindow exceeded the maximum of " + MaxWindows + " application windows.");

            aw.Parent = this;
            aw.Id = this.applicationWindows.Count;
            aw.Icon.Width = this.Width / IconColumns;
            aw.Icon.Height = aw.Icon.Width;

            var row = this.applicationWindows.Count / IconColumns;

            this.applicationWindows.Add(aw);

            this.iconStackPanels[row].Children.Clear();

            for (var col = 0; col < IconColumns; col++) {
                var index = row * IconColumns + col;
                if (index >= this.applicationWindows.Count)
                    break;

                var a = (ApplicationWindow)this.applicationWindows[index];
                if (a != null) {
                    this.iconStackPanels[row].Children.Add(a.Icon);
                    a.Icon.Click += this.Icon_Click;
                }
            }

            this.UpdateScreen();
        }

        private void Icon_Click(object sender, RoutedEventArgs e) {
            if (e.RoutedEvent.Name.CompareTo("TouchUpEvent") == 0) {
                var icon = (Icon)sender;
                var applicationWindow = (ApplicationWindow)this.applicationWindows[icon.Id];
                this.Child = applicationWindow.Open();
            }
        }

        public void Open() => this.Child = this.mainStackPanel;
    }
}
