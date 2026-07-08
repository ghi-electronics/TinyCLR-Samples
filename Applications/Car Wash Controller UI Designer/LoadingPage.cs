using System;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Threading;

namespace CarWashExample {
    // PORTED TO THE UI DESIGNER. Static layout (label + progress bar) is in LoadingPage.tcui; the timer
    // that advances the bar and navigates to the wash page stays here.
    public sealed partial class LoadingPage : Canvas {
        private readonly DispatcherTimer timer;

        public UIElement Elements => this;

        public LoadingPage() {
            InitializeComponent();

            this.timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            this.timer.Tick += this.OnTick;
        }

        public void Active() => this.timer.Start();
        public void Deactive() => this.timer.Stop();

        private void OnTick(object sender, EventArgs e) {
            this._progressBar.Value += 10;
            this._progressBar.Invalidate();

            if (this._progressBar.Value < this._progressBar.MaxValue)
                return;

            this.timer.Stop();
            this._progressBar.Value = 0;

            Program.NavigateTo(Program.CarWashPage.Elements);
            Program.CarWashPage.Active();
        }
    }
}
