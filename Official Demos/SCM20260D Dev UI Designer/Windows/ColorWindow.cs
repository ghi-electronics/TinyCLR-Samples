using System;
using System.Drawing;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using SystemDrawing = System.Drawing;

namespace Demos {
    public class ColorWindow : ApplicationWindow {
        private Canvas canvas;

        private const string Instruction1 = " This will test raw graphic on UD435 display: ";
        private const string Instruction2 = " Press Test button when you are ready.";

        private readonly Button testButton;
        private readonly Font font;
        private bool isRunning;
        private TextFlow textFlow;

        public ColorWindow(SystemDrawing.Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg11);

            this.testButton = new Button() {
                Child = new Text(this.font, "Test") {
                    ForeColor = Colors.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                Width = 100,
                Height = 30,
            };

            this.testButton.Click += this.TestButton_Click;
        }

        private void Initialize() {
            this.textFlow = new TextFlow();
            this.AppendInstruction(Instruction1);
            this.AppendInstruction(Instruction2);
        }

        private void AppendInstruction(string text) {
            this.textFlow.TextRuns.Add(text, this.font, Colors.White);
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);
        }

        private void Deinitialize() {
            this.textFlow.TextRuns.Clear();
            this.textFlow = null;
        }

        private void TestButton_Click(object sender, RoutedEventArgs e) {
            if (!this.isRunning) {
                this.ClearScreen();
                this.CreateWindow(false);
                this.textFlow.TextRuns.Clear();
                new Thread(this.ThreadTest).Start();
            }
        }

        protected override void Active() {
            this.Initialize();
            this.canvas = new Canvas();
            this.Child = this.canvas;
            this.isRunning = false;
            this.ClearScreen();
            this.CreateWindow(true);
        }

        private void OnButtonBack(object sender, RoutedEventArgs e) => this.Close();
        private void OnButtonNext(object sender, RoutedEventArgs e) => this.Close();

        protected override void Deactive() {
            this.isRunning = false;
            Thread.Sleep(10);
            this.canvas.Children.Clear();
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

                // Register touch event for Back / Next.
                this.OnBottomBarButtonBackTouchUpEvent += this.OnButtonBack;
                this.OnBottomBarButtonNextTouchUpEvent += this.OnButtonNext;
            }
        }

        private void CreateWindow(bool enableButton) {
            const int startX = 5;
            const int startY = 40;

            Canvas.SetLeft(this.textFlow, startX);
            Canvas.SetTop(this.textFlow, startY);
            this.canvas.Children.Add(this.textFlow);

            if (enableButton) {
                var buttonY = this.Height - ((this.testButton.Height * 3) / 2);
                Canvas.SetLeft(this.testButton, startX);
                Canvas.SetTop(this.testButton, buttonY);
                this.canvas.Children.Add(this.testButton);
            }
        }

        private void ThreadTest() {
            this.isRunning = true;

            var displayController = Display.DisplayController;
            var screen = SystemDrawing.Graphics.FromHdc(displayController.Hdc);
            var background = Resources.GetBitmap(Resources.BitmapResources.Color);

            const int screenWidth = 480;
            var screenHeight = 272 - this.TopBar.ActualHeight;
            var quarterWidth = screenWidth / 4;

            while (this.isRunning) {
                screen.Clear();
                screen.DrawImage(background, 0, 0);

                screen.FillEllipse(new SystemDrawing.SolidBrush(SystemDrawing.Color.FromArgb(100, 0xFF, 0, 0)), 0, 0, 100, 100);

                screen.FillRectangle(new SystemDrawing.SolidBrush(SystemDrawing.Color.FromArgb(100, 0, 0, 0xFF)),       0,                  120, quarterWidth, screenHeight - 120);
                screen.FillRectangle(new SystemDrawing.SolidBrush(SystemDrawing.Color.FromArgb(100, 0, 0xFF, 0)),       quarterWidth,       120, quarterWidth, screenHeight - 120);
                screen.FillRectangle(new SystemDrawing.SolidBrush(SystemDrawing.Color.FromArgb(100, 0xFF, 0, 0)),       quarterWidth * 2,   120, quarterWidth, screenHeight - 120);
                screen.FillRectangle(new SystemDrawing.SolidBrush(SystemDrawing.Color.FromArgb(100, 0xFF, 0xFF, 0xFF)), quarterWidth * 3,   120, quarterWidth, screenHeight - 120);

                screen.DrawString("This is blue",  this.font, new SystemDrawing.SolidBrush(SystemDrawing.Color.Blue),  0,                  screenHeight - 140);
                screen.DrawString("This is green", this.font, new SystemDrawing.SolidBrush(SystemDrawing.Color.Green), quarterWidth,       screenHeight - 140);
                screen.DrawString("This is red",   this.font, new SystemDrawing.SolidBrush(SystemDrawing.Color.Red),   quarterWidth * 2,   screenHeight - 140);
                screen.DrawString("This is white", this.font, new SystemDrawing.SolidBrush(SystemDrawing.Color.White), quarterWidth * 3,   screenHeight - 140);
                screen.DrawString("Touch here to return main menu [X]", this.font, new SystemDrawing.SolidBrush(SystemDrawing.Color.White), screenWidth / 2, 5);

                screen.Flush();

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            this.isRunning = false;
        }
    }
}
