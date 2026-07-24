using System.Collections;
using Demos.Properties;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using SystemDrawing = System.Drawing;

namespace Demos {
    // Files app. The static "tiles" layout lives in Apps\FilesApp.tcui (a DockPanel: a 40px chrome
    // spacer docked top, and a grid Canvas that fills the rest). This code-behind takes over that grid
    // Canvas and drives it from a small in-memory folder tree so you can TAP a folder to open it (and
    // tap Back to go up). Some folders are empty, like a real file browser.
    //
    // Each folder/file is drawn as ONE cell Canvas that contains several sub-elements (the two Border
    // shapes that make the icon, plus the name + subtitle Text). The TouchUp handler is wired on the
    // CELL, not on the individual shapes — TinyCLR.UI touch events bubble, so a tap anywhere in the cell
    // (on any of the icon pieces, on a label, or on the empty gap between them) reaches the one handler.
    // That's the robust way to make a multi-element icon behave as a single clickable item.
    public partial class FilesApp : DockPanel {
        // ---- fake file-system model ----
        private sealed class FsNode {
            public string Name;
            public bool IsFolder;
            public string Info; // file subtitle, e.g. "Text Document  ·  3 KB" (null for folders)
            public readonly ArrayList Children = new ArrayList(); // FsNode

            public FsNode(string name, bool isFolder, string info) {
                this.Name = name;
                this.IsFolder = isFolder;
                this.Info = info;
            }

            public FsNode Add(FsNode child) {
                this.Children.Add(child);
                return child;
            }
        }

        // Cell geometry: two columns, four rows (max 8 items on screen — no scrolling in this demo).
        private const int CellW = 260;
        private const int CellH = 56;
        private const int ColStride = 272;
        private const int RowStride = 64;
        private const int GridTop = 48;

        private static readonly Color FolderTab = Color.FromRgb(0xEF, 0xA9, 0x35);
        private static readonly Color FolderBody = Color.FromRgb(0xFC, 0xC6, 0x5A);
        private static readonly Color NameColor = Color.FromRgb(0x1D, 0x1D, 0x1F);
        private static readonly Color SubColor = Color.FromRgb(0x8A, 0x8A, 0x8E);
        private static readonly Color CrumbColor = Color.FromRgb(0x5B, 0x5B, 0x60);
        private static readonly Color LineColor = Color.FromRgb(0xE2, 0xE2, 0xE6);
        private static readonly Color LinkColor = Color.FromRgb(0x25, 0x63, 0xEB);
        private static readonly Color SelColor = Color.FromRgb(0xCC, 0xE4, 0xF7);

        private readonly SystemDrawing.Font nameFont;
        private readonly SystemDrawing.Font subFont;
        private readonly Canvas grid;                 // the file-grid Canvas from the .tcui (2nd DockPanel child)
        private readonly ArrayList path = new ArrayList(); // navigation stack of FsNode; [0] = root, last = current
        private FsNode selected;                      // highlighted file (folders navigate instead of selecting)

        public FilesApp() {
            InitializeComponent(); // builds the DockPanel frame: [0] = 40px top spacer, [1] = grid Canvas

            this.nameFont = Resources.GetFont(Resources.FontResources.droid_reg10);
            this.subFont = Resources.GetFont(Resources.FontResources.droid_reg09);

            // Take over the grid Canvas (the 2nd child; the 1st is the docked-top chrome spacer). We rebuild
            // its contents on every navigation, so the static designer folders are replaced by live ones.
            this.grid = (Canvas)this.Children[1];

            this.path.Add(BuildTree());
            this.Render();
        }

        // ---- the fake tree: nested folders, real-looking files, and a few empty folders ----
        private static FsNode BuildTree() {
            var root = new FsNode("This PC", true, null);

            var docs = root.Add(new FsNode("Documents", true, null));
            docs.Add(new FsNode("readme.txt", false, "Text Document  ·  3 KB"));
            docs.Add(new FsNode("budget.xlsx", false, "Excel Sheet  ·  12 KB"));
            docs.Add(new FsNode("notes.txt", false, "Text Document  ·  1 KB"));

            var fw = root.Add(new FsNode("Firmware", true, null));
            fw.Add(new FsNode("tinyclr_fw.bin", false, "BIN File  ·  512 KB"));
            fw.Add(new FsNode("bootloader.bin", false, "BIN File  ·  48 KB"));

            root.Add(new FsNode("Backups", true, null)); // empty folder

            var pics = root.Add(new FsNode("Pictures", true, null));
            var cam = pics.Add(new FsNode("Camera", true, null)); // nested folder
            cam.Add(new FsNode("img_001.jpg", false, "JPG Image  ·  2.1 MB"));
            cam.Add(new FsNode("img_002.jpg", false, "JPG Image  ·  1.8 MB"));
            pics.Add(new FsNode("logo.png", false, "PNG Image  ·  24 KB"));

            var logs = root.Add(new FsNode("Data Logs", true, null));
            logs.Add(new FsNode("log_2026_07.csv", false, "CSV File  ·  88 KB"));
            logs.Add(new FsNode("log_2026_06.csv", false, "CSV File  ·  91 KB"));
            logs.Add(new FsNode("Archive", true, null)); // empty nested folder

            var proj = root.Add(new FsNode("Projects", true, null));
            var tc = proj.Add(new FsNode("TinyCLR", true, null)); // nested folder
            tc.Add(new FsNode("Program.cs", false, "C# File  ·  4 KB"));
            tc.Add(new FsNode("MainWindow.tcui", false, "TCUI File  ·  2 KB"));
            proj.Add(new FsNode("Demo", true, null)); // empty folder

            root.Add(new FsNode("readme.txt", false, "Text Document  ·  3 KB"));
            root.Add(new FsNode("config.json", false, "JSON File  ·  1 KB"));

            return root;
        }

        private FsNode Current => (FsNode)this.path[this.path.Count - 1];

        private void NavigateInto(FsNode folder) {
            this.selected = null;
            this.path.Add(folder);
            this.Render();
        }

        private void NavigateBack() {
            if (this.path.Count > 1) {
                this.selected = null;
                this.path.RemoveAt(this.path.Count - 1);
                this.Render();
            }
        }

        // Rebuilds the grid Canvas for the current folder: breadcrumb (+ Back), a separator, then the item cells.
        private void Render() {
            this.grid.Children.Clear();

            var depth = this.path.Count - 1;
            var crumbLeft = 24;

            if (depth > 0) {
                var back = MakeBackCell();
                this.grid.Children.Add(back);
                Canvas.SetLeft(back, 20);
                Canvas.SetTop(back, 2);
                crumbLeft = 92;
            }

            this.grid.Children.Add(Positioned(
                new Text(this.nameFont, BuildBreadcrumb()) { ForeColor = CrumbColor, Width = 460, HorizontalAlignment = HorizontalAlignment.Left },
                crumbLeft, 6));

            var line = new Border { Width = 512, Height = 1, Background = new SolidColorBrush(LineColor), HorizontalAlignment = HorizontalAlignment.Left };
            line.SetBorderThickness(0);
            this.grid.Children.Add(Positioned(line, 24, 30));

            var items = this.Current.Children;
            if (items.Count == 0) {
                this.grid.Children.Add(Positioned(
                    new Text(this.nameFont, "This folder is empty") { ForeColor = SubColor, Width = 512, TextAlignment = TextAlignment.Center, HorizontalAlignment = HorizontalAlignment.Left },
                    24, 140));
            }
            else {
                for (var i = 0; i < items.Count && i < 8; i++) {
                    var node = (FsNode)items[i];
                    var cell = MakeCell(node);
                    this.grid.Children.Add(cell);
                    Canvas.SetLeft(cell, 24 + (i / 4) * ColStride);
                    Canvas.SetTop(cell, GridTop + (i % 4) * RowStride);
                }
            }

            // No explicit invalidate needed: Children.Clear()/Add() both call the owner's InvalidateMeasure(),
            // which re-runs measure -> arrange -> render for the new cells (and gives them bounds for hit-testing).
        }

        private string BuildBreadcrumb() {
            var s = "SD Card (A:)  ›  This PC";
            for (var i = 1; i < this.path.Count; i++) {
                s += "  ›  " + ((FsNode)this.path[i]).Name;
            }

            return s;
        }

        // One item cell: a sized Canvas holding the icon shapes + name/subtitle. TouchUp is wired on the
        // cell so a tap on ANY of its pieces (they bubble up) opens a folder / selects a file.
        private Canvas MakeCell(FsNode node) {
            var cell = new Canvas { Width = CellW, Height = CellH, HorizontalAlignment = HorizontalAlignment.Left };

            if (node == this.selected) {
                var hl = new Border { Width = CellW, Height = CellH, CornerRadius = 4, Background = new SolidColorBrush(SelColor), HorizontalAlignment = HorizontalAlignment.Left };
                hl.SetBorderThickness(0);
                cell.Children.Add(Positioned(hl, 0, 0));
            }

            if (node.IsFolder) {
                var tab = new Border { Width = 20, Height = 10, CornerRadius = 2, Background = new SolidColorBrush(FolderTab), HorizontalAlignment = HorizontalAlignment.Left };
                tab.SetBorderThickness(0);
                cell.Children.Add(Positioned(tab, 12, 4));

                var body = new Border { Width = 48, Height = 34, CornerRadius = 4, Background = new SolidColorBrush(FolderBody), HorizontalAlignment = HorizontalAlignment.Left };
                body.SetBorderThickness(0);
                cell.Children.Add(Positioned(body, 12, 10));
            }
            else {
                var page = new Border { Width = 34, Height = 44, CornerRadius = 3, Background = new SolidColorBrush(Colors.White), BorderBrush = new SolidColorBrush(Color.FromRgb(0xC8, 0xC8, 0xCC)), HorizontalAlignment = HorizontalAlignment.Left };
                page.SetBorderThickness(1);
                cell.Children.Add(Positioned(page, 19, 4));

                var corner = new Border { Width = 10, Height = 10, Background = new SolidColorBrush(Color.FromRgb(0xD8, 0xE4, 0xF0)), HorizontalAlignment = HorizontalAlignment.Left };
                corner.SetBorderThickness(0);
                cell.Children.Add(Positioned(corner, 43, 4));
            }

            cell.Children.Add(Positioned(
                new Text(this.nameFont, node.Name) { ForeColor = NameColor, Width = 180, HorizontalAlignment = HorizontalAlignment.Left }, 74, 12));

            var subtitle = node.IsFolder ? (node.Children.Count == 0 ? "Empty folder" : "File folder") : node.Info;
            cell.Children.Add(Positioned(
                new Text(this.subFont, subtitle) { ForeColor = SubColor, Width = 180, HorizontalAlignment = HorizontalAlignment.Left }, 74, 30));

            cell.TouchUp += (s, e) => {
                if (node.IsFolder) {
                    this.NavigateInto(node);
                }
                else {
                    this.selected = (this.selected == node) ? null : node;
                    this.Render();
                }

                e.Handled = true;
            };

            return cell;
        }

        private Canvas MakeBackCell() {
            var cell = new Canvas { Width = 64, Height = 22, HorizontalAlignment = HorizontalAlignment.Left };
            cell.Children.Add(Positioned(
                new Text(this.nameFont, "‹ Back") { ForeColor = LinkColor, Width = 60, HorizontalAlignment = HorizontalAlignment.Left }, 4, 3));
            cell.TouchUp += (s, e) => {
                this.NavigateBack();
                e.Handled = true;
            };
            return cell;
        }

        private static T Positioned<T>(T element, int left, int top) where T : UIElement {
            Canvas.SetLeft(element, left);
            Canvas.SetTop(element, top);
            return element;
        }
    }
}
