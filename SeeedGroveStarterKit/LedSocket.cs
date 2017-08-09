using GHIElectronics.TinyCLR.Devices.Gpio;
using System.Threading;

namespace SeeedGroveStarterKit {
    public class LedSocket {

        private GpioPin Pin;
        private Thread BlinkerT;
        private int BlinkDelay = 300;
        private void Blinker() {
            while (true) {
                Pin.Write(GpioPinValue.Low);
                Thread.Sleep(BlinkDelay);
                Pin.Write(GpioPinValue.High);
                Thread.Sleep(BlinkDelay);
            }
        }
        public LedSocket(int GpioPinNumber) {
            Pin = GpioController.GetDefault().OpenPin(GpioPinNumber);
            Pin.Write(GpioPinValue.Low);
            Pin.SetDriveMode(GpioPinDriveMode.Output);
            BlinkerT = new Thread(Blinker);
            BlinkerT.Start();
            BlinkerT.Suspend();
        }
        public void TurnOn() {
            lock (BlinkerT) {
                BlinkerT.Suspend();
                Pin.Write(GpioPinValue.High);
            }
        }
        public void TurnOff() {
            lock (BlinkerT) {
                BlinkerT.Suspend();
                Pin.Write(GpioPinValue.Low);
            }
        }
        public void Blink(double BlinkRateSec = 3) {
            BlinkDelay = (int)(((1 / BlinkRateSec) * 1000) / 2);
            lock (BlinkerT) {
                BlinkerT.Resume();
            }
        }
    }
}
