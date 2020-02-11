using System;
using System.Diagnostics;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Can;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;

namespace CAN {

    class Program {
        private static void Main() {
            var btn1 = GpioController.GetDefault().OpenPin(G80.GpioPin.PE0);
            btn1.SetDriveMode(GpioPinDriveMode.InputPullUp);

            var can = CanController.FromName(G80.CanBus.Can1);

            // Settings for 500kbps
            var propagation = 1;
            var phase1 = 12;
            var phase2 = 2;
            var baudratePrescaler = 6;
            var synchronizationJumpWidth = 1;
            var useMultiBitSampling = false;

            can.SetBitTiming(new CanBitTiming(propagation, phase1, phase2, baudratePrescaler, synchronizationJumpWidth, useMultiBitSampling));
            can.Enable();

            var message = new CanMessage() {
                Data = new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88 },
                ArbitrationId = 0x99,
                Length = 6,
                IsRemoteTransmissionRequest = false,
                IsExtendedId = false
            };

            can.MessageReceived += Can_MessageReceived;
            can.ErrorReceived += Can_ErrorReceived;

            while (true) {
                if (btn1.Read() == GpioPinValue.Low) can.WriteMessage(message);
                Thread.Sleep(100);
            }
        }

        private static void Can_MessageReceived(CanController sender, MessageReceivedEventArgs e) {
            sender.ReadMessage(out var message);

            Debug.WriteLine("Arbitration ID: 0x" + message.ArbitrationId.ToString("X8"));
            Debug.WriteLine("Is extended ID: " + message.IsExtendedId.ToString());
            Debug.WriteLine("Is remote transmission request: " + message.IsRemoteTransmissionRequest.ToString());
            Debug.WriteLine("Time stamp: " + message.Timestamp.ToString());

            var data = "";
            for (var i = 0; i < message.Length; i++) data += Convert.ToChar(message.Data[i]);

            Debug.WriteLine("Data: " + data);
        }

        private static void Can_ErrorReceived(CanController sender, ErrorReceivedEventArgs e) =>
            Debug.WriteLine("Error " + e.ToString());
    }
}