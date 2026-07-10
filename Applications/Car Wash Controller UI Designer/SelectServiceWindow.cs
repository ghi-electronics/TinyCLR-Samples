using System;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;

namespace CarWashExample {
    // PORTED TO THE UI DESIGNER. The whole layout — including the vehicle ComboBox and its items — is in
    // SelectServiceWindow.tcui; this partial only fills the runtime date and wires the service buttons.
    public sealed partial class SelectServiceWindow : Canvas {
        // Program.cs navigates with page.Elements; keep that contract (the page IS the canvas now).
        public UIElement Elements => this;

        public SelectServiceWindow() {
            InitializeComponent();

            this._dateText.TextContent =
                DateTime.Now.Day + " / " + DateTime.Now.Month + " / " + DateTime.Now.Year;
        }

        private void OnServiceButtonClick(object sender, RoutedEventArgs e) =>
            Program.NavigateTo(Program.PaymentPage.Elements);

  
    }
}
