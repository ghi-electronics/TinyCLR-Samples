using GHIElectronics.TinyCLR.Devices.I2c;

namespace Mikro.Click {
    class TouchClamp {
        #region MPR121_Defines
        // MPR121 Register Defines
        private const byte MHD_R = 0x2B;
        private const byte NHD_R = 0x2C;
        private const byte NCL_R = 0x2D;
        private const byte FDL_R = 0x2E;
        private const byte MHD_F = 0x2F;
        private const byte NHD_F = 0x30;
        private const byte NCL_F = 0x31;
        private const byte FDL_F = 0x32;
        private const byte ELE0_T = 0x41;
        private const byte ELE0_R = 0x42;
        private const byte ELE1_T = 0x43;
        private const byte ELE1_R = 0x44;
        private const byte ELE2_T = 0x45;
        private const byte ELE2_R = 0x46;
        private const byte ELE3_T = 0x47;
        private const byte ELE3_R = 0x48;
        private const byte ELE4_T = 0x49;
        private const byte ELE4_R = 0x4A;
        private const byte ELE5_T = 0x4B;
        private const byte ELE5_R = 0x4C;
        private const byte ELE6_T = 0x4D;
        private const byte ELE6_R = 0x4E;
        private const byte ELE7_T = 0x4F;
        private const byte ELE7_R = 0x50;
        private const byte ELE8_T = 0x51;
        private const byte ELE8_R = 0x52;
        private const byte ELE9_T = 0x53;
        private const byte ELE9_R = 0x54;
        private const byte ELE10_T = 0x55;
        private const byte ELE10_R = 0x56;
        private const byte ELE11_T = 0x57;
        private const byte ELE11_R = 0x58;
        private const byte FIL_CFG = 0x5D;
        private const byte ELE_CFG = 0x5E;
        private const byte GPIO_CTRL0 = 0x73;
        private const byte GPIO_CTRL1 = 0x74;
        private const byte GPIO_DATA = 0x75;
        private const byte GPIO_DIR = 0x76;
        private const byte GPIO_EN = 0x77;
        private const byte GPIO_SET = 0x78;
        private const byte GPIO_CLEAR = 0x79;
        private const byte GPIO_TOGGLE = 0x7A;
        private const byte ATO_CFG0 = 0x7B;
        private const byte ATO_CFGU = 0x7D;
        private const byte ATO_CFGL = 0x7E;
        private const byte ATO_CFGT = 0x7F;


        // Global Constants
        private const byte TOU_THRESH = 16;
        private const byte REL_THRESH = 10;
        #endregion

        public const int NO_EVENT = -1;
        private const ushort MPR121WriteAddress = 0xB4; // From the documentation
        //private const int MPR121ClockRate = 100; // kHz
        //private const int defaultTimeout = 1000; // Not sure what the units are

        private I2cDevice i2cBus;

        public TouchClamp(I2cController controller) {

            var settings = new I2cConnectionSettings(MPR121WriteAddress >> 1) {
                BusSpeed = 400_000,
            };
            this.i2cBus = controller.GetDevice(settings);

            this.Configure();
        }

        /// <summary>
        /// Write data to the MPR-121, 
        /// </summary>
        /// <param name="data">the first byte of the data must be the 
        /// MPR-121 register to be written to.</param>
        /// <returns>number of bytes sent</returns>
        private int WriteTo(byte[] data) {
            this.i2cBus.Write(data);

            return 0;//
        }

        /// <summary>
        /// Return the byte at the requested address. 
        /// </summary>
        /// <param name="address">The address of the MPR-121 register to read</param>
        /// <returns>The value read. Note that 0 could mean either 0 bytes or failure</returns>
        private byte[] buffer1 = new byte[1];
        public byte ReadByte(byte address) {
            this.buffer1[0] = address;

            this.i2cBus.WriteRead(this.buffer1, this.buffer1);

            return this.buffer1[0];
        }

        /// <summary>
        /// Return the currently pressed key if there is one. Else return -1.
        /// </summary>
        /// <returns> -1  there is no discernable key touched
        ///            0-11 the key that has been touched.
        ///            The number corresponds to the number printed on the board.
        /// </returns>
        public int GetKey() {
            var key = NO_EVENT;
            var status = (this.ReadByte(1) << 8 | this.ReadByte(0));
            if (status > 0) {
                for (key = 0; (key < 12) && ((status & (1 << key)) == 0); key++) {
                    ;
                }
            }
            return key;
        }

        /// <summary>
        /// Configure the IC on the keyboard translated from the code provided by 
        /// Sparkfun.
        /// </summary>
        public void Configure() {
            var bytesSent = 0;
            // Section A
            // This group controls filtering when data is > baseline.
            bytesSent = this.WriteTo(new byte[] { MHD_R, 0x01 });

            bytesSent = this.WriteTo(new byte[] { NHD_R, 0x01 });

            bytesSent = this.WriteTo(new byte[] { NCL_R, 0x00 });

            bytesSent = this.WriteTo(new byte[] { FDL_R, 0x00 });

            // Section B
            // This group controls filtering when data is < baseline.
            bytesSent = this.WriteTo(new byte[] { MHD_F, 0x01 });

            bytesSent = this.WriteTo(new byte[] { NHD_F, 0x01 });

            bytesSent = this.WriteTo(new byte[] { NCL_F, 0xFF });

            bytesSent = this.WriteTo(new byte[] { FDL_F, 0x02 });

            // Section C
            // This group sets touch and release thresholds for each electrode
            bytesSent = this.WriteTo(new byte[] { ELE0_T, TOU_THRESH });

            bytesSent = this.WriteTo(new byte[] { ELE0_R, REL_THRESH });

            bytesSent = this.WriteTo(new byte[] { ELE1_T, TOU_THRESH });

            bytesSent = this.WriteTo(new byte[] { ELE1_R, REL_THRESH });

            bytesSent = this.WriteTo(new byte[] { ELE2_T, TOU_THRESH });

            bytesSent = this.WriteTo(new byte[] { ELE2_R, REL_THRESH });

            bytesSent = this.WriteTo(new byte[] { ELE3_T, TOU_THRESH });

            bytesSent = this.WriteTo(new byte[] { ELE3_R, REL_THRESH });

            bytesSent = this.WriteTo(new byte[] { ELE4_T, TOU_THRESH });

            bytesSent = this.WriteTo(new byte[] { ELE4_R, REL_THRESH });

            bytesSent = this.WriteTo(new byte[] { ELE5_T, TOU_THRESH });

            bytesSent = this.WriteTo(new byte[] { ELE5_R, REL_THRESH });

            bytesSent = this.WriteTo(new byte[] { ELE6_T, TOU_THRESH });

            bytesSent = this.WriteTo(new byte[] { ELE6_R, REL_THRESH });

            bytesSent = this.WriteTo(new byte[] { ELE7_T, TOU_THRESH });

            bytesSent = this.WriteTo(new byte[] { ELE7_R, REL_THRESH });

            bytesSent = this.WriteTo(new byte[] { ELE8_T, TOU_THRESH });

            bytesSent = this.WriteTo(new byte[] { ELE8_R, REL_THRESH });

            bytesSent = this.WriteTo(new byte[] { ELE9_T, TOU_THRESH });

            bytesSent = this.WriteTo(new byte[] { ELE9_R, REL_THRESH });

            bytesSent = this.WriteTo(new byte[] { ELE10_T, TOU_THRESH });

            bytesSent = this.WriteTo(new byte[] { ELE10_R, REL_THRESH });

            bytesSent = this.WriteTo(new byte[] { ELE11_T, TOU_THRESH });

            bytesSent = this.WriteTo(new byte[] { ELE11_R, REL_THRESH });

            // Section D
            // Set the Filter Configuration
            // Set ESI2

            bytesSent = this.WriteTo(new byte[] { FIL_CFG, 0x04 });

            // Section E
            // Electrode Configuration
            // Enable 6 Electrodes and set to run mode
            // Set ELE_CFG to 0x00 to return to standby mode

            bytesSent = this.WriteTo(new byte[] { ELE_CFG, 0x0C });    // Enables all 12 Electrodes        
        }
    }
}
