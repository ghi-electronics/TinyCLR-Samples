using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using Demos.Properties;
using Demos.Utils;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media.Imaging;

namespace Demos {
    public class ApplicationWindow {
        public int Id { get => this.Icon.Id; set => this.Icon.Id = value; }
        public Icon Icon { get; set; }
        public MainWindow Parent { get; set; }

        public bool EnableClockOnTopBar { get; set; }
        public bool EnableButtomBack { get; set; }
        public bool EnableButtomNext { get; set; }

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

        private bool actived;

        static BottomBar sBottomBar;
        static TopBar sTopBar;

        public ApplicationWindow(Bitmap icon, string title, int width, int height) {
            this.Icon = new Icon(icon, title);

            this.Width = width;
            this.Height = height;

            if (sBottomBar == null)
                sBottomBar = new BottomBar(this.Width, true, true);

            if (sTopBar == null)
                sTopBar = new TopBar(this.Width, "", true);
        }

        public UIElement TopBar => this.topBar?.Child;
        public UIElement BottomBar => this.bottomBar?.Child;

        protected virtual void Active() {

        }

        protected virtual void Deactive() {

        }

        public UIElement Open() {
            if (!this.actived) {

                // Avoid defregment if next screen allocate big memory
                GC.Collect();
                GC.WaitForPendingFinalizers();

                //Debug.WriteLine("" + GHIElectronics.TinyCLR.Native.Memory.ManagedMemory.FreeBytes / 1024);


                this.topBar = sTopBar;// new TopBar(this.Width, this.Icon.IconText, this.EnableClockOnTopBar);
                //this.topBar.OnClose += this.OnClose;

                if (this.EnableButtomBack || this.EnableButtomNext) {
                    try {
                        this.bottomBar = sBottomBar;// new BottomBar(this.Width, this.EnableButtomBack, this.EnableButtomNext);


                    }
                    catch {
                        //if (this.topBar != null)
                        //    this.topBar.Dispose();

                        //if (this.bottomBar != null)
                        //    this.bottomBar.Dispose();

                        return null;
                    }
                }


                this.Active();

                if (this.EnableButtomBack || this.EnableButtomNext) {
                    this.Child.AddHandler(Buttons.ButtonUpEvent, new RoutedEventHandler(this.OnButtonUp), true);
                    this.Child.IsVisibleChanged += this.Child_IsVisibleChanged;

                    if (this.EnableButtomBack) {
                        this.bottomBar.ButtonBack.Click += this.ButtonBack_Click;
                    }

                    if (this.EnableButtomNext) {
                        this.bottomBar.ButtonNext.Click += this.ButtonNext_Click;
                    }
                }

                this.actived = true;
            }



            return this.Child;
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e) {
            if (e.RoutedEvent.Name.CompareTo("TouchUpEvent") == 0) {
                OnBottomBarButtonBackTouchUpEvent?.Invoke(sender, e);
            }
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e) {
            if (e.RoutedEvent.Name.CompareTo("TouchUpEvent") == 0) {
                OnBottomBarButtonNextTouchUpEvent?.Invoke(sender, e);
            }
        }

        private void Child_IsVisibleChanged(object sender, PropertyChangedEventArgs e) {
            var isVisible = (bool)e.NewValue;

            if (this.Child != null && isVisible)
                Buttons.Focus(this.Child);
        }

        public void Close() {
            if (this.actived) {
                this.Deactive();

                //if (this.topBar != null)
                //    this.topBar.Dispose();

                //if (this.bottomBar != null)
                //    this.bottomBar.Dispose();

                //if (this.bottomBar != null) {
                //    this.bottomBar.ButtonBack.Click -= this.ButtonBack_Click;
                //    this.bottomBar.ButtonNext.Click -= this.ButtonNext_Click;
                //}

                this.actived = false;
            }

            if (this.Parent != null) {
                this.Parent.Open();
            }
        }

        private void OnButtonUp(object sender, RoutedEventArgs e) => OnBottomBarButtonUpEvent?.Invoke(sender, e);

        private void OnClose(object sender, RoutedEventArgs e) => this.Close();

        public void SetEnableButtonNext(bool enable) {
            //if (this.bottomBar.ButtonNext == null)
            //    return;

            //this.bottomBar.ButtonNext.Visibility = enable ? Visibility.Visible : Visibility.Collapsed;

        }

        public void UpdateStatusText(TextFlow textFlow, string text, Font font, bool clearscreen) => this.UpdateStatusText(textFlow, text, font, clearscreen, Color.White);

        public void UpdateStatusText(TextFlow textFlow, string text, Font font, bool clearscreen, Color color) {

            var timeout = 100;
            var count = 0;

            if (textFlow == null)
                goto _return;

            lock (textFlow) {

                count = textFlow.TextRuns.Count + 2;
            }

            Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(timeout), _ => {

                if (textFlow == null)
                    return null;

                lock (textFlow) {
                    if (clearscreen)
                        textFlow.TextRuns.Clear();

                    textFlow.TextRuns.Add(text, font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(color.R, color.G, color.B));
                    textFlow.TextRuns.Add(TextRun.EndOfLine);

                    return null;
                }

            }, null);

            if (textFlow == null)
                goto _return;

            lock (textFlow) {

                if (clearscreen) {
                    while (textFlow.TextRuns.Count < 2) {
                        Thread.Sleep(1);
                    }
                }
                else {
                    while (textFlow.TextRuns.Count < count) {
                        Thread.Sleep(1);
                    }
                }
            }
_return:
            GC.Collect();
            GC.WaitForPendingFinalizers();

        }
    }
}
