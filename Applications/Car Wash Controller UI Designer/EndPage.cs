using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;

namespace CarWashExample {
    // PORTED TO THE UI DESIGNER. The whole layout is in EndPage.tcui; only the Done handler is here.
    public sealed partial class EndPage : Canvas {
        public UIElement Elements => this;

        public EndPage() => InitializeComponent();

        public void Active() { }
        public void Deactive() { }

        private void OnDoneClick(object sender, RoutedEventArgs e) =>
            Program.NavigateTo(Program.SelectServicePage.Elements);
    }
}
