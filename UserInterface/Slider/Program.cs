using System;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media.Imaging;
using GHIElectronics.TinyCLR.UI.Input;


namespace SliderDemo {
    public sealed class Program : Application {
        public DisplayController disp;

        private Program(DisplayController d) : base(d) => this.disp = d;
        public static void Main() {
            var disp = DisplayController.GetDefault();
            disp.SetConfiguration(new ParallelDisplayControllerSettings {
                // Your display configuration
                // This is for G120E Dev Board
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

            var backlight = GpioController.GetDefault().OpenPin(G120E.GpioPin.P3_17);
            backlight.SetDriveMode(GpioPinDriveMode.Output);
            backlight.Write(GpioPinValue.High);

            var app = new Program(disp);

            // The buttons
            var cont = GpioController.GetDefault();
            var right = cont.OpenPin(G120E.GpioPin.P2_22);
            var left = cont.OpenPin(G120E.GpioPin.P2_21);
            var select = cont.OpenPin(G120E.GpioPin.P2_25);
            var up = cont.OpenPin(G120E.GpioPin.P2_10);
            var down = cont.OpenPin(G120E.GpioPin.P0_22);
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

            app.Run(new MainMenuWindow(disp));

        }
    }

    internal sealed class MainMenuWindow : Window {

        // This member keeps the menu item panel around
        private Slider m_MenuItemPanel;

        public MainMenuWindow(DisplayController disp)
            : base() {
            this.Width = disp.ActiveConfiguration.Width;
            this.Height = disp.ActiveConfiguration.Height;
            this.Visibility = Visibility.Visible;

            this.m_MenuItemPanel = new Slider(disp.ActiveConfiguration.Width, disp.ActiveConfiguration.Height, Resource.GetBitmap(Resource.BitmapResources.Desc_Icon).Width) {
                Background = new SolidColorBrush(Colors.Red),
                Font = Resource.GetFont(Resource.FontResources.NinaB)
            };
            // The top child contains the menu items
            // We pass in the small bitmap, large bitmap a description and then a large bitmap to use
            // as a common sized bitmap for calculating the width and height of a MenuItem
            var menuItem1 = new SliderItem(
                BitmapImage.FromGraphics(System.Drawing.Graphics.FromImage(
                Resource.GetBitmap(Resource.BitmapResources.Desc_Icon_Small))),
                BitmapImage.FromGraphics(System.Drawing.Graphics.FromImage(
                Resource.GetBitmap(Resource.BitmapResources.Desc_Icon)))
                , "Demo Description");
            var menuItem2 = new SliderItem(BitmapImage.FromGraphics(System.Drawing.Graphics.FromImage(
                Resource.GetBitmap(Resource.BitmapResources.PWM_Icon_Small))),
                BitmapImage.FromGraphics(System.Drawing.Graphics.FromImage(
                Resource.GetBitmap(Resource.BitmapResources.PWM_Icon)))
                , "PWM Example");
            var menuItem3 = new SliderItem(BitmapImage.FromGraphics(System.Drawing.Graphics.FromImage(
               Resource.GetBitmap(Resource.BitmapResources.Piezo_Icon_Small))),
               BitmapImage.FromGraphics(System.Drawing.Graphics.FromImage(
               Resource.GetBitmap(Resource.BitmapResources.Piezo_Icon)))
               , "Piezo Example");

            this.m_MenuItemPanel.AddMenuItem(menuItem1);
            this.m_MenuItemPanel.AddMenuItem(menuItem2);
            this.m_MenuItemPanel.AddMenuItem(menuItem3);

            this.Child = this.m_MenuItemPanel;
            Buttons.Focus(this);

        }

        protected override void OnButtonDown(ButtonEventArgs e) {
            switch (e.Button) {
                case HardwareButton.Select: {
                        switch (this.m_MenuItemPanel.CurrentChild) {
                            case 0:  // description
                                // Run the demo
                                // ...
                                break;
                            case 1:  // PWM

                                break;
                        }
                    }
                    break;

                case HardwareButton.Left: {
                        if (this.m_MenuItemPanel != null)
                            this.m_MenuItemPanel.CurrentChild--;
                    }
                    break;

                case HardwareButton.Right: {
                        if (this.m_MenuItemPanel != null)
                            this.m_MenuItemPanel.CurrentChild++;
                    }
                    break;
            }
            base.OnButtonDown(e);
        }
    }
}
