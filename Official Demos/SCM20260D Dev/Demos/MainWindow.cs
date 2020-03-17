using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;
using GHIElectronics.TinyCLR.UI.Shapes;
using GHIElectronics.TinyCLR.UI.Threading;

namespace Demos {
    public sealed class MainWindow : Window {               

        private UIElement topBar;
        private readonly StackPanel mainStackPanel;        
        private StackPanel[] iconStackPanels;

        private int registerWindowIndex = 0;

        const int IconColum = 6;
        const int IconRow = 3;
        const int MaxWindows = IconColum * IconRow;

        private ApplicationWindow[] applicationWindows;

        public MainWindow(int width, int height) : base() {
            this.Width = width;
            this.Height = height;

            this.mainStackPanel = new StackPanel(Orientation.Vertical);            
            this.Child = this.mainStackPanel;

            this.Background = new LinearGradientBrush(Colors.Blue, Colors.Teal, 0, 0, width, height);

            this.CreateTopBar();
            this.CreateIcons();            
        }

        private void CreateTopBar() {            
            var topbar = new TopBar(this.Width, "Demo App", false);
            this.topBar = topbar.Element;
        }

        private void CreateIcons() {
            this.iconStackPanels = new StackPanel[IconRow];
            this.applicationWindows = new ApplicationWindow[IconRow * IconColum];

            for (var r = 0; r < IconRow; r++) {
                this.iconStackPanels[r] = new StackPanel(Orientation.Horizontal);
            }
        }

        private void UpdateScreen() {
            this.mainStackPanel.Children.Clear();
            this.mainStackPanel.Children.Add(this.topBar);

            for (var r = 0; r < IconRow; r++) {
                this.mainStackPanel.Children.Add(this.iconStackPanels[r]);
            }

            this.Invalidate();
        }

        public void RegisterWindow(ApplicationWindow aw) {
            if (this.registerWindowIndex == MaxWindows)
                throw new ArgumentOutOfRangeException("No more than " + MaxWindows + " windows");
            aw.Parent = this;
            this.applicationWindows[this.registerWindowIndex] = aw;
            this.applicationWindows[this.registerWindowIndex].Id = this.registerWindowIndex;

            var r = this.registerWindowIndex / IconColum;

            this.iconStackPanels[r].Children.Clear();

            for (var i = 0; i < IconColum; i++) {
                var a = this.applicationWindows[r * IconColum + i];

                if (a != null) {
                    this.iconStackPanels[r].Children.Add(a.Icon);                    
                    a.Icon.Click += this.Icon_Click;
                }
            }

            this.UpdateScreen();

            this.registerWindowIndex++;
        }

        private void Icon_Click(object sender, RoutedEventArgs e) {
            if (e.RoutedEvent.Name.CompareTo("TouchUpEvent") == 0) {
                var icon = (Icon)sender;

                var applicationWindow = this.applicationWindows[icon.Id];

                this.Child = applicationWindow.Open();                
            }
        }

        public void Open() => this.Child = this.mainStackPanel;
    }
}
