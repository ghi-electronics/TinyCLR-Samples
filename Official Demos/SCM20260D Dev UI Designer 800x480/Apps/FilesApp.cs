using GHIElectronics.TinyCLR.UI.Controls;

namespace Demos {
    // Files app fragment. Layout is in Apps\FilesApp.tcui; the TreeView is a fake
    // SD/USB file browser. No runtime logic — the nodes are declared in the .tcui.
    // Root layout is a DockPanel (title docked top, tree fills) — see FilesApp.tcui.
    public partial class FilesApp : DockPanel {
        public FilesApp() => InitializeComponent();
    }
}
