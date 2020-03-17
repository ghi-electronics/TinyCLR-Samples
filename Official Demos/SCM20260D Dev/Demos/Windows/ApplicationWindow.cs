using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Media.Imaging;

namespace Demos {
    public class ApplicationWindow {
        public int Id { get=>this.Icon.Id; set => this.Icon.Id = value; }
        public Icon Icon { get; set; }        
        public MainWindow Parent { get; set; }

        protected TopBar TopBar { get; set; }
        protected UIElement Element { get; set; }
        protected int Width { get; set; }
        protected int Height { get; set; }

        private bool actived; 

        public ApplicationWindow(Bitmap icon, string iconText, int width, int height) {            
            this.Icon = new Icon(icon, iconText);

            this.Width = width;
            this.Height = height;

            this.TopBar = new TopBar(this.Width, iconText);
        }

        protected virtual void Active() {

        }

        protected virtual void Deactive() {

        }

        public UIElement Open() {
            if (!this.actived) {
                this.Active();

                this.actived = true;
            }

            return this.Element;
        }

        public void Close() {
            if (this.actived) {
                this.Deactive();

                this.actived = false;
            }

            if (this.Parent != null) {
                this.Parent.Open();
            }
        }
    }
}
