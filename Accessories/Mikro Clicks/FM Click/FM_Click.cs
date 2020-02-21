using System;
using System.Threading;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Gpio;


namespace Click.Radio {
    /// <summary>RadioFM module for .NET Gadgeteer</summary>
    
    public class RadioFM1 {
        /// <summary>The channel returned by <see cref="Seek" /> when no Channel is found.</summary>
        public const double INVALID_CHANNEL = -1.0;

        /// <summary>The minimum volume the device can output.</summary>
        public const int MIN_VOLUME = 0;

        /// <summary>The maximum volume the device can output.</summary>
        public const int MAX_VOLUME = 255;
        private const byte I2C_ADDRESS = 0x10;
        private const byte REGISTER_DEVICEID = 0x00;
        private const byte REGISTER_CHIPID = 0x01;
        private const byte REGISTER_POWERCFG = 0x02;
        private const byte REGISTER_CHANNEL = 0x03;
        private const byte REGISTER_SYSCONFIG1 = 0x04;
        private const byte REGISTER_SYSCONFIG2 = 0x05;
        private const byte REGISTER_STATUSRSSI = 0x0A;
        private const byte REGISTER_READCHAN = 0x0B;
        private const byte REGISTER_RDSA = 0x0C;
        private const byte REGISTER_RDSB = 0x0D;
        private const byte REGISTER_RDSC = 0x0E;
        private const byte REGISTER_RDSD = 0x0F;

        //Register 0x02 - POWERCFG
        private const byte BIT_SMUTE = 15;
        private const byte BIT_DMUTE = 14;
        private const byte BIT_SKMODE = 10;
        private const byte BIT_SEEKUP = 9;
        private const byte BIT_SEEK = 8;

        //Register 0x03 - CHANNEL
        private const byte BIT_TUNE = 15;

        //Register 0x04 - SYSCONFIG1
        private const byte BIT_RDS = 12;
        private const byte BIT_DE = 11;

        //Register 0x05 - SYSCONFIG2
        private const byte BIT_SPACE1 = 5;
        private const byte BIT_SPACE0 = 4;

        //Register 0x0A - STATUSRSSI
        private const byte BIT_RDSR = 15;
        private const byte BIT_STC = 14;
        private const byte BIT_SFBL = 13;
        private const byte BIT_AFCRL = 12;
        private const byte BIT_RDSS = 11;
        private const byte BIT_STEREO = 8;
        private const int RADIO_TEXT_GROUP_CODE = 2;
        private const int TOGGLE_FLAG_POSITION = 5;
        private const int CHARS_PER_SEGMENT = 2;
        private const int MAX_MESSAGE_LENGTH = 64;
        private const int MAX_SEGMENTS = 16;
        private const int MAX_CHARS_PER_GROUP = 4;
        private const int VERSION_A_TEXT_SEGMENT_PER_GROUP = 2;
        private const int VERSION_B_TEXT_SEGMENT_PER_GROUP = 1;
        private int currentVolume;
        private bool radioTextWorkerRunning;
        private Thread radioTextWorkerThread;
        private string currentRadioText;
        private I2cDevice i2cBus;
        GpioPin resetPin;
        GpioPin selPin;
        private int spacingDivisor;
        private int baseChannel;
        private ushort[] registers;
        private RadioTextChangedHandler onRadioTextChanged;

        /// <summary>Represents the delegate that is used to handle the <see cref="RadioTextChanged" /> event.</summary>
        /// <param name="sender">The <see cref="RadioFM1" /> that raised the event.</param>
        /// <param name="newRadioText">The new Radio Text.</param>
        public delegate void RadioTextChangedHandler(RadioFM1 sender, string newRadioText);

        /// <summary>Raised when new Radio Text is available.</summary>
        public event RadioTextChangedHandler RadioTextChanged;

        /// <summary>Gets or sets the Volume of the radio.</summary>
      
        public int Volume {
            get {
                return this.currentVolume;
            }

            set {
                // if (value > RadioFM1.MAX_VOLUME || value < RadioFM1.MIN_VOLUME) throw new ArgumentOutOfRangeException("value", "The volume provided was outside the allowed range.");

                this.currentVolume = value;
                this.SetDeviceVolume((ushort)value);//(ushort)System.Math.Ceiling(value / 100.0));
            }
        }

        /// <summary>The maximum channel the radio and be tuned to.</summary>
        public double MaxChannel {
            private set;
            get;
        }

        /// <summary>The minimum channel the radio and be tuned to.</summary>
        public double MinChannel {
            private set;
            get;
        }

        /// <summary>Gets or sets the Channel of the radio.</summary>
        public double Channel {
            get {
                return this.GetDeviceChannel() / 10.0;
            }

            set {
                if (value > this.MaxChannel || value < this.MinChannel) throw new ArgumentOutOfRangeException("value", "The Channel provided was outside the allowed range.");

                this.SetDeviceChannel((int)(value * 10));
                this.currentRadioText = "N/A";
            }
        }

        /// <summary>Gets the current Radio Text.</summary>
        public string RadioText {
            get {
                return this.currentRadioText;
            }
        }

        /// <summary>The enumeration that determines which direction to Seek when calling Seek(direction);</summary>
        public enum SeekDirection {

            /// <summary>Seeks for a higher station number.</summary>
            Forward,

            /// <summary>Seeks for a lower station number.</summary>
            Backward
        };

        /// <summary>The radio frequency band.</summary>
        public enum Band {

            /// <summary>The band used in the United States and Europe (87.5-108MHz).</summary>
            USAEurope,

            /// <summary>The wide band used in the Japan (76-108MHz).</summary>
            JapanWide,

            /// <summary>The band used in Japan (76-90MHz).</summary>
            Japan
        }

        /// <summary>The radio channel spacing.</summary>
        public enum Spacing {

            /// <summary>Spacing in USA and Austrailia (200KHz).</summary>
            USAAustrailia,

            /// <summary>Spacing in Europe and Japan (100KHz).</summary>
            EuropeJapan
        }

        /// <summary>Constructs a new instance.</summary>      
        public RadioFM1(int reset,int chipSelect ,string deviceBus) {
            this.radioTextWorkerRunning = true;
            this.currentRadioText = "N/A";
            this.spacingDivisor = 2;
            this.baseChannel = 875;
            this.registers = new ushort[16];

            this.resetPin = GpioController.GetDefault().OpenPin(reset);
            this.resetPin.SetDriveMode(GpioPinDriveMode.Output);

            this.selPin = GpioController.GetDefault().OpenPin(chipSelect);
            this.selPin.SetDriveMode(GpioPinDriveMode.Output); 
            this.selPin.Write(GpioPinValue.High);
  
            var settings = new I2cConnectionSettings(RadioFM1.I2C_ADDRESS,400);           
            var controller = I2cController.FromName(deviceBus);
            this.i2cBus = controller.GetDevice(settings);       
            this.InitializeDevice();
            this.SetChannelConfiguration(Spacing.USAAustrailia, Band.USAEurope);
            this.Channel = this.MinChannel;
            //this.Volume = RadioFM1.MAX_VOLUME;
            this.radioTextWorkerThread = new Thread(this.RadioTextWorker);
        }

        /// <summary>Tells the radio to Seek in the given direction until it finds a station.</summary>
        /// <param name="direction">The direction to Seek the radio.</param>
        /// <remarks>It does wrap around when seeking.</remarks>
        /// <returns>The Channel that was tuned to or <see cref="INVALID_CHANNEL" /> if no Channel was found.</returns>
        public double Seek(SeekDirection direction) {
            this.currentRadioText = "N/A";

            if (this.SeekDevice(direction))
                return this.Channel;
            else
                return RadioFM1.INVALID_CHANNEL;
        }

        /// <summary>Increases the Volume by one.</summary>
        public void IncreaseVolume() {
            if (this.Volume == RadioFM1.MAX_VOLUME) return;

            ++this.Volume;
        }

        /// <summary>Decreases the Volume by one.</summary>
        public void DecreaseVolume() {
            if (this.Volume == RadioFM1.MIN_VOLUME) return;

            --this.Volume;
        }

        /// <summary>Sets the channel spacing and band of the device.</summary>
        /// <param name="spacing">The channel spacing.</param>
        /// <param name="band">The channel base band.</param>
        public void SetChannelConfiguration(Spacing spacing, Band band) {
            this.ReadRegisters();

            if (spacing == Spacing.USAAustrailia) {
                this.registers[RadioFM1.REGISTER_SYSCONFIG2] &= 0xFFCF;
                this.spacingDivisor = 2;
            } else if (spacing == Spacing.EuropeJapan) {
                this.registers[RadioFM1.REGISTER_SYSCONFIG2] &= 0xFFDF;
                this.spacingDivisor = 1;
            } else {
                throw new ArgumentException("You must provide a valid spacing.", "spacing");
            }

            if (band == Band.USAEurope) {
                this.registers[RadioFM1.REGISTER_SYSCONFIG2] &= 0xFF3F;
                this.baseChannel = 875;
                this.MinChannel = 87.5;
                this.MaxChannel = 107.5;
            } else if (band == Band.JapanWide) {
                this.registers[RadioFM1.REGISTER_SYSCONFIG2] &= 0xFF7F;
                this.baseChannel = 760;
                this.MinChannel = 76;
                this.MaxChannel = 108;
            } else if (band == Band.Japan) {
                this.registers[RadioFM1.REGISTER_SYSCONFIG2] &= 0xFFBF;
                this.baseChannel = 760;
                this.MinChannel = 76;
                this.MaxChannel = 90;
            } else {
                throw new ArgumentException("You must provide a valid band.", "band");
            }

            this.UpdateRegisters();
        }

        private void OnRadioTextChanged(RadioFM1 sender, string newRadioText) {
            var e = this.RadioTextChanged;

            if (e != null)
                e(sender, newRadioText);
        }

        private void RadioTextWorker() {
            char[] currentRadioText = new char[RadioFM1.MAX_MESSAGE_LENGTH];
            bool[] isSegmentPresent = new bool[RadioFM1.MAX_SEGMENTS];
            int endOfMessage = -1;
            int endSegmentAddress = -1;
            string lastMessage = "";
            int lastTextToggleFlag = -1;
            bool waitForNextMessage = false;

            while (this.radioTextWorkerRunning) {
                this.ReadRegisters();
                ushort a = this.registers[RadioFM1.REGISTER_RDSA];
                ushort b = this.registers[RadioFM1.REGISTER_RDSB];
                ushort c = this.registers[RadioFM1.REGISTER_RDSC];
                ushort d = this.registers[RadioFM1.REGISTER_RDSD];
                bool ready = (this.registers[RadioFM1.REGISTER_STATUSRSSI] & (1 << RadioFM1.BIT_RDSR)) != 0;

                if (ready) {
                    int programIDCode = a;
                    int groupTypeCode = b >> 12;
                    int versionCode = (b >> 11) & 0x1;
                    int trafficIDCode = (b >> 10) & 0x1;
                    int programTypeCode = (b >> 5) & 0x1F;

                    if (groupTypeCode == RadioFM1.RADIO_TEXT_GROUP_CODE) {
                        int textToggleFlag = b & (1 << (RadioFM1.TOGGLE_FLAG_POSITION - 1));
                        if (textToggleFlag != lastTextToggleFlag) {
                            currentRadioText = new char[RadioFM1.MAX_MESSAGE_LENGTH];
                            lastTextToggleFlag = textToggleFlag;
                            waitForNextMessage = false;
                        } else if (waitForNextMessage) {
                            continue;
                        }

                        int segmentAddress = (b & 0xF);
                        int textAddress = -1;
                        isSegmentPresent[segmentAddress] = true;

                        if (versionCode == 0) {
                            textAddress = segmentAddress * RadioFM1.CHARS_PER_SEGMENT * RadioFM1.VERSION_A_TEXT_SEGMENT_PER_GROUP;
                            currentRadioText[textAddress] = (char)(c >> 8);
                            currentRadioText[textAddress + 1] = (char)(c & 0xFF);
                            currentRadioText[textAddress + 2] = (char)(d >> 8);
                            currentRadioText[textAddress + 3] = (char)(d & 0xFF);
                        } else {
                            textAddress = segmentAddress * RadioFM1.CHARS_PER_SEGMENT * RadioFM1.VERSION_B_TEXT_SEGMENT_PER_GROUP;
                            currentRadioText[textAddress] = (char)(d >> 8);
                            currentRadioText[textAddress + 1] = (char)(d & 0xFF);
                        }

                        if (endOfMessage == -1) {
                            for (int i = 0; i < RadioFM1.MAX_CHARS_PER_GROUP; ++i) {
                                if (currentRadioText[textAddress + i] == 0xD) {
                                    endOfMessage = textAddress + i;
                                    endSegmentAddress = segmentAddress;
                                }
                            }
                        }

                        if (endOfMessage == -1)
                            continue;

                        bool complete = true;
                        for (int i = 0; i < endSegmentAddress; ++i)
                            if (!isSegmentPresent[i])
                                complete = false;

                        if (!complete)
                            continue;

                        string message = new string(currentRadioText, 0, endOfMessage);
                        if (message == lastMessage) {
                            this.currentRadioText = message;
                            this.OnRadioTextChanged(this, message);
                            waitForNextMessage = true;

                            for (int i = 0; i < endSegmentAddress; ++i)
                                isSegmentPresent[i] = false;

                            endOfMessage = -1;
                            endSegmentAddress = -1;
                        }

                        lastMessage = message;
                    }

                    Thread.Sleep(35);
                } else {
                    Thread.Sleep(40);
                }
            }
        }

        private void InitializeDevice() {
            this.resetPin.Write(GpioPinValue.Low);
            Thread.Sleep(100);
            this.resetPin.Write(GpioPinValue.High);
            Thread.Sleep(10);

            this.ReadRegisters();
            this.registers[0x07] = 0x8100;
            this.UpdateRegisters();

            Thread.Sleep(500);

            this.ReadRegisters();
            this.registers[RadioFM1.REGISTER_POWERCFG] = 0x4001;
            this.registers[RadioFM1.REGISTER_SYSCONFIG1] |= (1 << RadioFM1.BIT_RDS);
            this.registers[RadioFM1.REGISTER_SYSCONFIG2] &= 0xFFCF;
            this.registers[RadioFM1.REGISTER_SYSCONFIG2] &= 0xFFF0;
            this.registers[RadioFM1.REGISTER_SYSCONFIG2] |= 0x000F;
            this.UpdateRegisters();

            Thread.Sleep(110);
        }

        private void ReadRegisters() {
            byte[] data = new byte[32];


            this.i2cBus.Read(data);
            
          
            for (int i = 0, x = 0xA; i < 12; i += 2, ++x)
                this.registers[x] = (ushort)((data[i] << 8) | (data[i + 1]));

            for (int i = 12, x = 0x0; i < 32; i += 2, ++x)
                this.registers[x] = (ushort)((data[i] << 8) | (data[i + 1]));
        }

        private void UpdateRegisters() {
            byte[] data = new byte[12];

            for (int x = 0x02, i = 0; x < 0x08; ++x, i += 2) {
                data[i] = (byte)(this.registers[x] >> 8);
                data[i + 1] = (byte)(this.registers[x] & 0x00FF);
            }

            this.i2cBus.Write(data);
           

        }

        private void SetDeviceVolume(ushort volume) {
            this.ReadRegisters();
            this.registers[RadioFM1.REGISTER_SYSCONFIG2] &= 0xFFF0;
            this.registers[RadioFM1.REGISTER_SYSCONFIG2] |= volume;
            this.UpdateRegisters();
        }

        private int GetDeviceChannel() {
            this.ReadRegisters();

            int Channel = this.registers[RadioFM1.REGISTER_READCHAN] & 0x03FF;

            return Channel * this.spacingDivisor + this.baseChannel;
        }

        private void SetDeviceChannel(int newChannel) {
            newChannel -= this.baseChannel;
            newChannel /= this.spacingDivisor;

            this.ReadRegisters();
            this.registers[RadioFM1.REGISTER_CHANNEL] &= 0xFE00;
            this.registers[RadioFM1.REGISTER_CHANNEL] |= (ushort)newChannel;
            this.registers[RadioFM1.REGISTER_CHANNEL] |= (1 << RadioFM1.BIT_TUNE);
            this.UpdateRegisters();

            while (true) {
                this.ReadRegisters();
                if ((this.registers[RadioFM1.REGISTER_STATUSRSSI] & (1 << BIT_STC)) != 0)
                    break;
            }

            this.ReadRegisters();
            this.registers[RadioFM1.REGISTER_CHANNEL] &= 0x7FFF;
            this.UpdateRegisters();

            while (true) {
                this.ReadRegisters();
                if ((this.registers[RadioFM1.REGISTER_STATUSRSSI] & (1 << BIT_STC)) == 0)
                    break;
            }
        }

        private bool SeekDevice(SeekDirection direction) {
            this.ReadRegisters();

            this.registers[RadioFM1.REGISTER_POWERCFG] &= 0xFBFF;

            if (direction == SeekDirection.Backward)
                this.registers[RadioFM1.REGISTER_POWERCFG] &= 0xFDFF;
            else
                this.registers[RadioFM1.REGISTER_POWERCFG] |= 1 << RadioFM1.BIT_SEEKUP;

            this.registers[RadioFM1.REGISTER_POWERCFG] |= (1 << RadioFM1.BIT_SEEK);
            this.UpdateRegisters();

            while (true) {
                this.ReadRegisters();
                if ((this.registers[RadioFM1.REGISTER_STATUSRSSI] & (1 << RadioFM1.BIT_STC)) != 0)
                    break;
            }

            this.ReadRegisters();
            int valueSFBL = this.registers[RadioFM1.REGISTER_STATUSRSSI] & (1 << RadioFM1.BIT_SFBL);
            this.registers[RadioFM1.REGISTER_POWERCFG] &= 0xFEFF;
            this.UpdateRegisters();

            while (true) {
                this.ReadRegisters();
                if ((this.registers[RadioFM1.REGISTER_STATUSRSSI] & (1 << RadioFM1.BIT_STC)) == 0)
                    break;
            }

            if (valueSFBL > 0)
                return false;

            return true;
        }
    }
}
