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

        const int IconColum = 4;
        const int IconRow = 2;
        const int MaxWindows = IconColum * IconRow;

        private ArrayList applicationWindows;

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

            for (var r = 0; r < IconRow; r++) {
                this.mainStackPanel.Children.Add(this.iconStackPanels[r]);
            }

            this.Invalidate();
        }

        public void RegisterWindow(ApplicationWindow aw) {
            if (this.applicationWindows.Count == MaxWindows)
                throw new ArgumentOutOfRangeException("No more than " + MaxWindows + " windows");
            

            aw.Parent = this;
            aw.Id = this.applicationWindows.Count;

            var r = this.applicationWindows.Count / IconColum;

            this.applicationWindows.Add(aw);

            this.iconStackPanels[r].Children.Clear();

            for (var i = 0; i < IconColum; i++) {
                if (r * IconColum + i >= this.applicationWindows.Count)
                    break;

                var a = (ApplicationWindow)this.applicationWindows[r * IconColum + i];

                if (a != null) {
                    this.iconStackPanels[r].Children.Add(a.Icon);                    
                }
            }

            this.UpdateScreen();
            
        }

        private void OnButtonUp(object sender, RoutedEventArgs e) {
            var buttonSource = (GHIElectronics.TinyCLR.UI.Input.ButtonEventArgs)e;
            
            switch (buttonSource.Button) {
                case GHIElectronics.TinyCLR.UI.Input.HardwareButton.Left:
                    if (this.selectWindowIndex == 0) {
                        this.selectWindowIndex = this.applicationWindows.Count - 1;
                    }
                    else {
                        this.selectWindowIndex--;
                    }
                    break;

                case GHIElectronics.TinyCLR.UI.Input.HardwareButton.Right:
                    if (this.selectWindowIndex == this.applicationWindows.Count - 1) {
                        this.selectWindowIndex = 0;
                    }
                    else {
                        this.selectWindowIndex++;
                    }
                    break;

                case GHIElectronics.TinyCLR.UI.Input.HardwareButton.Select:
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    var applicationWindow = (ApplicationWindow)this.applicationWindows[this.selectWindowIndex];

                    this.Child = applicationWindow.Open();
                    break;

            }

            Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(1), _ => { this.UpdateScreen(); return null; }, null);

        }

        public void Open() {
                GC.Collect();
                GC.WaitForPendingFinalizers();

                this.Child = this.mainStackPanel;     
        }

    }
}
