using System;
using System.Drawing;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using SystemDrawing = System.Drawing;

namespace Demos {
    // PORTED TO THE UI DESIGNER. The layout (status TextFlow + Test button) is designed in
    // QspiContent.tcui; this window stays an ApplicationWindow (so it keeps its place in the menu
    // navigation), hosts that fragment, and drives its elements with the same QSPI test logic as before.
    public class QspiWindow : ApplicationWindow {
        private Canvas canvas;

        private const string Instruction1 = "This test will Erase/Write/Read:";
        private const string Instruction2 = " - 8 first sectors";
        private const string Instruction3 = " - 8 last sectors";
        private const string Instruction4 = "All existing data on these sectors will be erased!";
        private const string Instruction5 = "Press Test button when you are ready.";

        private readonly Font font;
        private bool isRunning;
        private TextFlow textFlow;   // the designed StatusText, grabbed from the fragment
        private Button testButton;   // the designed TestButton, grabbed from the fragment

        public QspiWindow(SystemDrawing.Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg11);
        }

        protected override void Active() {
            var content = new QspiContent();
            this.canvas = content;
            this.textFlow = content.StatusText;
            this.testButton = content.TestButton;
            this.testButton.Click += this.TestButton_Click;
            this.isRunning = false;

            this.AddChrome();
            this.Child = this.canvas;

            this.textFlow.TextRuns.Clear();
            this.AppendInstruction(Instruction1);
            this.AppendInstruction(Instruction2);
            this.AppendInstruction(Instruction3);
            this.AppendInstruction(Instruction4);
            this.AppendInstruction(Instruction5);
        }

        // Adds the shared TopBar/BottomBar chrome (unchanged from the original) to the fragment's canvas.
        private void AddChrome() {
            if (this.TopBar != null) {
                Canvas.SetLeft(this.TopBar, 0);
                Canvas.SetTop(this.TopBar, 0);
                this.canvas.Children.Add(this.TopBar);
            }

            if (this.BottomBar != null) {
                Canvas.SetLeft(this.BottomBar, 0);
                Canvas.SetTop(this.BottomBar, this.Height - this.BottomBar.Height);
                this.canvas.Children.Add(this.BottomBar);

                // Register touch event for Back / Next.
                this.OnBottomBarButtonBackTouchUpEvent += this.OnButtonBack;
                this.OnBottomBarButtonNextTouchUpEvent += this.OnButtonNext;
            }
        }

        private void AppendInstruction(string text) {
            this.textFlow.TextRuns.Add(text, this.font, Colors.White);
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);
        }

        private void TestButton_Click(object sender, RoutedEventArgs e) {
            if (!this.isRunning) {
                this.testButton.Visibility = Visibility.Collapsed; // hide while running (original removed it)
                this.textFlow.TextRuns.Clear();
                new Thread(this.ThreadTest).Start();
            }
        }

        private void OnButtonBack(object sender, RoutedEventArgs e) => this.Close();
        private void OnButtonNext(object sender, RoutedEventArgs e) => this.Close();

        protected override void Deactive() {
            this.isRunning = false;
            Thread.Sleep(10);
            this.canvas.Children.Clear();
        }

        private void ThreadTest() {
            this.isRunning = true;

            var storeController = StorageController.FromName(SC20260.StorageController.QuadSpi);
            var drive = storeController.Provider;

            drive.Open();
            try {
                var sectorSize = drive.Descriptor.RegionSizes[0];

                var textWrite = Encoding.UTF8.GetBytes("this is for test");
                var dataRead = new byte[sectorSize];
                var dataWrite = new byte[sectorSize];

                for (var i = 0; i < sectorSize; i += textWrite.Length) {
                    Array.Copy(textWrite, 0, dataWrite, i, textWrite.Length);
                }

                // Two rounds: first 8 sectors, then the last 8 sectors.
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
