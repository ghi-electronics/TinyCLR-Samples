using GHIElectronics.TinyCLR.Devices.Gpio;
using System.Threading;

namespace SeeedGroveStarterKit {
    public class LedSocket {

        private GpioPin Pin;
        private Thread BlinkerT;
        private int BlinkDelay = 300;
        private void Blinker() {
            while (true) {
                this.Pin.Write(GpioPinValue.Low);
                Thread.Sleep(this.BlinkDelay);
                this.Pin.Write(GpioPinValue.High);
                Thread.Sleep(this.BlinkDelay);
            }
        }
        public LedSocket(int GpioPinNumber) {
            this.Pin = GpioController.GetDefault().OpenPin(GpioPinNumber);
            this.Pin.Write(GpioPinValue.Low);
            this.Pin.SetDriveMode(GpioPinDriveMode.Output);
            this.BlinkerT = new Thread(this.Blinker);
            this.BlinkerT.Start();
            this.BlinkerT.Suspend();
        }
        public void TurnOn() {
            lock (this.BlinkerT) {
                this.BlinkerT.Suspend();
                this.Pin.Write(GpioPinValue.High);
            }
        }
        public void TurnOff() {
            lock (this.BlinkerT) {
                this.BlinkerT.Suspend();
                this.Pin.Write(GpioPinValue.Low);
            }
        }
        public void Blink(double BlinkRateSec = 3) {
            this.BlinkDelay = (int)(((1 / BlinkRateSec) * 1000) / 2);
            lock (this.BlinkerT) {
                this.BlinkerT.Resume();
            }
        }
    }
}
