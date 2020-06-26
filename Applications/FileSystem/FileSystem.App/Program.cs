using uAlfat.Core;
using GHIElectronics.TinyCLR.Pins;
using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace FileSystem.App
{
    class Program
    {
        static void Main()
        {
            //use UART5 on SITCore SC20260

            //alfat testing
            //var module = new AlfatModule(SC20260.UartPort.Uart5, SC20260.StorageController.UsbHostMassStorage, SC20260.StorageController.SdCard);

            //uAlfat testing
            var module = new uAlfatModule(SC20260.UartPort.Uart5, SC20260.StorageController.UsbHostMassStorage, SC20260.StorageController.SdCard);
            module.Run();
        }
    }
}
