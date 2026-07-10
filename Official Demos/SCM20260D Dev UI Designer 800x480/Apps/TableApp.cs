using GHIElectronics.TinyCLR.UI.Controls;

namespace Demos {
    // ADC Channels table app fragment. Layout is in Apps\TableApp.tcui; this fills the DataGrid
    // with columns and a set of fake ADC-channel rows. The grid itself (ctor width/rowHeight/
    // rowCount/font) is created by the generated InitializeComponent().
    public partial class TableApp : Canvas {
        public TableApp() {
            InitializeComponent();

            // Columns: widths sum to the grid width (520). AddColumn(DataGridColumn(label, width)).
            this._channelGrid.AddColumn(new DataGridColumn("Channel", 200));
            this._channelGrid.AddColumn(new DataGridColumn("Pin", 180));
            this._channelGrid.AddColumn(new DataGridColumn("mV", 140));

            // Fake ADC channels. Each row is a DataGridItem whose object[] length must match the
            // column count (3). AddItem appends the row.
            var rows = new string[][] {
                new string[] { "A0", "PA0", "1650" },
                new string[] { "A1", "PA1", "812"  },
                new string[] { "A2", "PA2", "3300" },
                new string[] { "A3", "PA3", "45"   },
                new string[] { "A4", "PA4", "2210" },
                new string[] { "A5", "PA5", "1024" },
                new string[] { "A6", "PA6", "1980" },
                new string[] { "A7", "PA7", "500"  },
            };

            for (var i = 0; i < rows.Length; i++)
                this._channelGrid.AddItem(new DataGridItem(new object[] { rows[i][0], rows[i][1], rows[i][2] }));
        }
    }
}
