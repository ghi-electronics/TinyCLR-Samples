using System;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using SystemDrawing = System.Drawing;

namespace Demos {
    internal class QspiWindow : ApplicationWindow {
        private Canvas canvas;
        private SystemDrawing.Font font;
        private TextFlow textFlow;
        private bool isRunning;

        private const string Instruction1 = "This test will Erase/Write/Read:";
        private const string Instruction2 = " - 8 first sectors";
        private const string Instruction3 = " - 8 last sectors";
        private const string Instruction4 = "All existing data on these sectors";
        private const string Instruction5 = "will be erased!";

        public QspiWindow(Resources.BitmapResources icon, string text, int width, int height) : base(icon, text, width, height) {
        }

        private void Initialize() {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg08);
            this.textFlow = new TextFlow();
            this.AppendInstruction(Instruction1);
            this.AppendInstruction(Instruction2);
            this.AppendInstruction(Instruction3);
            this.AppendInstruction(Instruction4);
            this.AppendInstruction(Instruction5);
        }

        private void AppendInstruction(string text) {
            this.textFlow.TextRuns.Add(text, this.font, Colors.White);
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);
        }

        private void Deinitialize() {
            if (this.BottomBar != null)
                this.OnBottomBarButtonUpEvent -= this.OnHardwareButtonUp;

            this.textFlow.TextRuns.Clear();
            this.canvas.Children.Clear();
            this.font.Dispose();
            this.textFlow = null;
            this.canvas = null;
        }

        protected override void Active() {
            this.Initialize();
            this.canvas = new Canvas();
            this.Child = this.canvas;
            this.ClearScreen();
            this.CreateWindow();
        }

        protected override void Deactive() {
            this.isRunning = false;
            Thread.Sleep(10);
            this.Deinitialize();
        }

        private void ClearScreen() {
            this.canvas.Children.Clear();

            if (this.TopBar != null) {
                Canvas.SetLeft(this.TopBar, 0);
                Canvas.SetTop(this.TopBar, 0);
                this.canvas.Children.Add(this.TopBar);
            }

            if (this.BottomBar != null) {
                Canvas.SetLeft(this.BottomBar, 0);
                Canvas.SetTop(this.BottomBar, this.Height - this.BottomBar.Height);
                this.canvas.Children.Add(this.BottomBar);
                this.OnBottomBarButtonUpEvent += this.OnHardwareButtonUp;
            }
        }

        private void OnHardwareButtonUp(object sender, RoutedEventArgs e) {
            var buttonSource = (ButtonEventArgs)e;
            switch (buttonSource.Button) {
                case HardwareButton.Left:
                    this.Close();
                    break;
                case HardwareButton.Right:
                case HardwareButton.Select:
                    if (!this.isRunning)
                        new Thread(this.ThreadTest).Start();
                    break;
            }
        }

        private void CreateWindow() {
            Canvas.SetLeft(this.textFlow, 5);
            Canvas.SetTop(this.textFlow, 20);
            this.canvas.Children.Add(this.textFlow);
        }

        private void ThreadTest() {
            this.isRunning = true;

            var storeController = StorageController.FromName(SC20100.StorageController.QuadSpi);
            var drive = storeController.Provider;
            drive.Open();

            try {
                var sectorSize = drive.Descriptor.RegionSizes[0];
                var textWrite = Encoding.UTF8.GetBytes("this is for test");
                var dataRead = new byte[sectorSize];
                var dataWrite = new byte[sectorSize];

                for (var i = 0; i < sectorSize; i += textWrite.Length)
                    Array.Copy(textWrite, 0, dataWrite, i, textWrite.Length);

                int[] sectorStarts = { 0, 4088 };
                foreach (var startSector in sectorStarts) {
                    var endSector = startSector + 8;
                    for (var s = startSector; s < endSector; s++) {
                        var address = s * sectorSize;

                        this.UpdateStatusText("Erasing sector " + s, true);
                        drive.Erase(address, sectorSize, TimeSpan.FromSeconds(100));

                        this.UpdateStatusText("Writing sector " + s, false);
                        drive.Write(address, sectorSize, dataWrite, 0, TimeSpan.FromSeconds(100));

                        this.UpdateStatusText("Reading sector " + s, false);
                        drive.Read(address, sectorSize, dataRead, 0, TimeSpan.FromSeconds(100));

                        for (var idx = 0; idx < sectorSize; idx++) {
                            if (dataRead[idx] != dataWrite[idx]) {
                                this.UpdateStatusText("Compare failed at: " + idx, false);
                                return;
                            }
                        }
                    }
                }

                this.UpdateStatusText("Tested Quad Spi successful!", false);
            }
            finally {
                drive.Close();
                this.isRunning = false;
            }
        }

        private void UpdateStatusText(string text, bool clearScreen) =>
            this.UpdateStatusText(this.textFlow, text, this.font, clearScreen, SystemDrawing.Color.White);
    }
}
