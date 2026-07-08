using CarWashExample.Properties;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using SystemDrawing = System.Drawing;

namespace CarWashExample {
    // PORTED TO THE UI DESIGNER. Static layout is in PaymentWindow.tcui; this partial keeps the
    // on-screen-keyboard font setup and the Back/Next handlers (Next shows a confirm MessageBox).
    public sealed partial class PaymentWindow : Canvas {
        private readonly SystemDrawing.Font fontNinaB = Resources.GetFont(Resources.FontResources.NinaB);

        public UIElement Elements => this;

        public PaymentWindow() {
            // The on-screen keyboard is global state — keep it on a font that matches this page so the
            // popup doesn't visually clash when the user taps into a TextBox.
            OnScreenKeyboard.Font = this.fontNinaB;

            InitializeComponent();
        }

        private void OnBackClick(object sender, RoutedEventArgs e) =>
            Program.NavigateTo(Program.SelectServicePage.Elements);

        private void OnGoClick(object sender, RoutedEventArgs e) {
            var result = MessageBox.Show(this, "Are you sure?", "Confirm",
                MessageBox.MessageBoxButtons.YesNo, this.fontNinaB);

            if (result != MessageBox.DialogResult.Yes)
                return;

            Program.NavigateTo(Program.LoadingPage.Elements);
            Program.LoadingPage.Active();
        }
    }
}
