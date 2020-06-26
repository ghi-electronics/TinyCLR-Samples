using GHIElectronics.TinyCLR.Devices.Uart;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace uAlfat.Core
{
    public class CommunicationsBus : IDisposable
    {
        private string TempData { set; get; } = string.Empty;
        private CommunicationsProtocal protocal;
        private StreamReader commandFile;
        private UartController serialPort;
        private bool disposed;
        public event IncomingDataEventHandler DataReceived;
        public delegate void IncomingDataEventHandler(string data);
        public enum CommunicationsProtocal
        {
            Uart,
            Spi,
            I2C
        }
        public void SetBaudRate(int baudRate) {

            var uartSetting = new UartSetting() {
                BaudRate = baudRate,
                DataBits = 8,
                Parity = UartParity.None,
                StopBits = UartStopBitCount.One,
                Handshaking = UartHandshake.None

            };

            this.serialPort.SetActiveSettings(uartSetting);
        }
        public CommunicationsBus(string uartPort,int baudRate = 9600) {
            this.disposed = false;
            this.commandFile = null;
            this.protocal = CommunicationsProtocal.Uart;

            this.serialPort = UartController.FromName(uartPort);

            var uartSetting = new UartSetting() {
                BaudRate = baudRate,
                DataBits = 8,
                Parity = UartParity.None,
                StopBits = UartStopBitCount.One,
                Handshaking = UartHandshake.None

            };

            this.serialPort.SetActiveSettings(uartSetting);

            this.serialPort.WriteBufferSize = 16 * 1024;
            this.serialPort.ReadBufferSize = 16 * 1024;

            this.serialPort.Enable();

            this.serialPort.DataReceived += this.SerialPort_DataReceived;

        }
        private void SerialPort_DataReceived(UartController sender, DataReceivedEventArgs e)
        {
            var rxBuffer = new byte[e.Count];
            var bytesReceived = this.serialPort.Read(rxBuffer, 0, e.Count);
            var dataStr = Encoding.UTF8.GetString(rxBuffer, 0, bytesReceived);
            if (uAlfatModule.IsEchoEnabled) {
                for (var i = 0; i < rxBuffer.Length; i++) {
                    // send back whatever the host sent except for terminal line                    
                    this.serialPort.Write(rxBuffer, i, 1);
                }
            }

            //Debug.WriteLine(dataStr);
            this.TempData += dataStr;
            if (dataStr.IndexOf(Strings.NewLine) > -1)
            {
                DataReceived?.Invoke(this.TempData.Trim());
                this.TempData = string.Empty;
            }
            
        }
        public CommunicationsBus(string portName, StreamReader commandFile) : this(portName) => this.commandFile = commandFile;

        public void Dispose()
        {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
                return;

            if (disposing)
            {
                if (this.commandFile != null)
                {
                    this.commandFile.Close();
                    this.commandFile.Dispose();
                    this.commandFile = null;
                }

                if (this.serialPort != null)
                {
                    this.serialPort.Disable();
                    this.serialPort.Dispose();
                    this.serialPort = null;
                }
            }

            this.disposed = true;
        }

        ~CommunicationsBus()
        {
            this.Dispose(false);
        }

        public void Write(string data)
        {
            data = data.Trim();

            switch (this.protocal)
            {
                case CommunicationsProtocal.I2C:
                    throw new NotImplementedException();

                case CommunicationsProtocal.Spi:
                    throw new NotImplementedException();

                case CommunicationsProtocal.Uart:
                    var buffer = Encoding.UTF8.GetBytes(data);
                    this.serialPort.Write(buffer, 0, buffer.Length);

                    break;
            }
        }

        public int ByteToWrite => this.serialPort != null ? this.serialPort.BytesToRead : 0;
        public void Write(byte[] data) => this.Write(data, 0, data.Length);

        public void Write(byte[] data, int offset, int count) {
            switch (this.protocal) {
                case CommunicationsProtocal.I2C:
                    throw new NotImplementedException();

                case CommunicationsProtocal.Spi:
                    throw new NotImplementedException();

                case CommunicationsProtocal.Uart:

                    this.serialPort.Write(data, offset, count);

                    break;
            }
        }

        public void WriteLine(string line)
        {
            line = line.Trim();

            switch (this.protocal)
            {
                case CommunicationsProtocal.I2C:
                    throw new NotImplementedException();

                case CommunicationsProtocal.Spi:
                    throw new NotImplementedException();

                case CommunicationsProtocal.Uart:
                    
                    var databytes = UTF8Encoding.UTF8.GetBytes(line+ Strings.NewLine);
                    this.serialPort.Write(databytes);

                    break;
            }
        }

        public bool ProcessCommandFromFile()
        {
            if (this.commandFile == null) throw new InvalidOperationException("This object was not initialized for a command file.");

            var line = this.commandFile.ReadLine(); //Possible make this non-blocking
            if (line == null)
                return false;

            this.WriteLine(line);

            return true;
        }

        public string ReadLine()
        {
            if (this.serialPort != null)
            {
                if (this.serialPort.BytesToRead > 0)
                {
                    var rxBuffer = new byte[this.serialPort.BytesToRead];
                    var bytesReceived = this.serialPort.Read(rxBuffer, 0, this.serialPort.BytesToRead);
                    var dataStr = Encoding.UTF8.GetString(rxBuffer, 0, bytesReceived);
                    return dataStr;
                }
                 
            }
            else if (this.commandFile != null)
            {
                return this.commandFile.ReadLine(); //Handle end of file
            }
            else
            {
                throw new NotImplementedException();
            }
            return string.Empty;
        }

        public void Read(byte[] data) => this.Read(data, 0, data.Length);

        public void Read(byte[] data, int offset, int count) {
            if (data == null)
                throw new ArgumentNullException();

            if (offset + count > data.Length)
                throw new ArgumentOutOfRangeException();

            if (this.serialPort != null) {
                var read = 0;
                while (read < count) {
                    read += this.serialPort.Read(data, offset + read, count - read);
                }
            }
            else if (this.commandFile != null) {
                throw new NotImplementedException();
            }
            else {
                throw new NotImplementedException();
            }
        }
    }
}
