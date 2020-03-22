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

        const int IconColum = 3;
        const int IconRow = 1;

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

            var r = 0;
            var showListLeft = new ArrayList();

            this.iconStackPanels[r].Children.Clear();
            this.iconStackPanels[r].SetMargin(0, (this.Height - this.topBar.Height - ((ApplicationWindow)this.applicationWindows[0]).Icon.Height)/2, 0, 0);

            var left = IconColum / 2;
            var right = IconColum / 2;

            var end = this.applicationWindows.Count-1;

            for (var i = 0; i < left; i++) {
                if (this.selectWindowIndex > 0) {
                    showListLeft.Add(this.selectWindowIndex - 1 - i);
                    
                }
                else {
                    showListLeft.Add(end - i);
                }
            }

            var showListRight = new ArrayList();

            for (var i = 0; i < right; i++) {
                if (this.selectWindowIndex < end) {
                    showListRight.Add(this.selectWindowIndex + 1 + i);

                }
                else {
                    showListRight.Add(i);
                }
            }


            for (var i = left-1; i >=0; i--) {
                var a = (ApplicationWindow)this.applicationWindows[(int)showListLeft[i]];

                this.iconStackPanels[r].Children.Add(a.Icon);
            }

            this.iconStackPanels[r].Children.Add(((ApplicationWindow)this.applicationWindows[this.selectWindowIndex]).Icon);

            for (var i = 0; i < right; i++) {
                var a = (ApplicationWindow)this.applicationWindows[(int)showListRight[i]];

                this.iconStackPanels[r].Children.Add(a.Icon);
            }


            for (r = 0; r < IconRow; r++) {
                this.mainStackPanel.Children.Add(this.iconStackPanels[r]);
            }

            this.Invalidate();
        }

        public void RegisterWindow(ApplicationWindow aw) {

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
