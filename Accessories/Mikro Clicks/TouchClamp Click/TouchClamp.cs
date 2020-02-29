using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;

namespace TouchClamp_Click {
    class TouchClamp {
        #region MPR121_Defines
        // MPR121 Register Defines
        private const Byte MHD_R = 0x2B;
        private const Byte NHD_R = 0x2C;
        private const Byte NCL_R = 0x2D;
        private const Byte FDL_R = 0x2E;
        private const Byte MHD_F = 0x2F;
        private const Byte NHD_F = 0x30;
        private const Byte NCL_F = 0x31;
        private const Byte FDL_F = 0x32;
        private const Byte ELE0_T = 0x41;
        private const Byte ELE0_R = 0x42;
        private const Byte ELE1_T = 0x43;
        private const Byte ELE1_R = 0x44;
        private const Byte ELE2_T = 0x45;
        private const Byte ELE2_R = 0x46;
        private const Byte ELE3_T = 0x47;
        private const Byte ELE3_R = 0x48;
        private const Byte ELE4_T = 0x49;
        private const Byte ELE4_R = 0x4A;
        private const Byte ELE5_T = 0x4B;
        private const Byte ELE5_R = 0x4C;
        private const Byte ELE6_T = 0x4D;
        private const Byte ELE6_R = 0x4E;
        private const Byte ELE7_T = 0x4F;
        private const Byte ELE7_R = 0x50;
        private const Byte ELE8_T = 0x51;
        private const Byte ELE8_R = 0x52;
        private const Byte ELE9_T = 0x53;
        private const Byte ELE9_R = 0x54;
        private const Byte ELE10_T = 0x55;
        private const Byte ELE10_R = 0x56;
        private const Byte ELE11_T = 0x57;
        private const Byte ELE11_R = 0x58;
        private const Byte FIL_CFG = 0x5D;
        private const Byte ELE_CFG = 0x5E;
        private const Byte GPIO_CTRL0 = 0x73;
        private const Byte GPIO_CTRL1 = 0x74;
        private const Byte GPIO_DATA = 0x75;
        private const Byte GPIO_DIR = 0x76;
        private const Byte GPIO_EN = 0x77;
        private const Byte GPIO_SET = 0x78;
        private const Byte GPIO_CLEAR = 0x79;
        private const Byte GPIO_TOGGLE = 0x7A;
        private const Byte ATO_CFG0 = 0x7B;
        private const Byte ATO_CFGU = 0x7D;
        private const Byte ATO_CFGL = 0x7E;
        private const Byte ATO_CFGT = 0x7F;


        // Global Constants
        private const Byte TOU_THRESH = 16;
        private const Byte REL_THRESH = 10;
        #endregion

        public const int NO_EVENT = -1;
        private const ushort MPR121WriteAddress = 0xB4; // From the documentation
        private const int MPR121ClockRate = 100; // kHz
        private const int defaultTimeout = 1000; // Not sure what the units are

        private I2cDevice i2cBus;

        public TouchClamp(string device) {

            var settings = new I2cConnectionSettings(MPR121WriteAddress >> 1) {
                BusSpeed = 400,
            };
            this.i2cBus = I2cController.FromName(device).GetDevice(settings);

            configure();
        }

        /// <summary>
        /// Write data to the MPR-121, 
        /// </summary>
        /// <param name="data">the first byte of the data must be the 
        /// MPR-121 register to be written to.</param>
        /// <returns>number of bytes sent</returns>
        private int writeTo(byte[] data) {
            i2cBus.Write(data);

            return 0;//
        }

        /// <summary>
        /// Return the byte at the requested address. 
        /// </summary>
        /// <param name="address">The address of the MPR-121 register to read</param>
        /// <returns>The value read. Note that 0 could mean either 0 bytes or failure</returns>
        private byte[] buffer1 = new byte[1];
        public byte readByte(byte address) {
            buffer1[0] = address;

            i2cBus.WriteRead(buffer1, buffer1);

            return buffer1[0];
        }

        /// <summary>
        /// Return the currently pressed key if there is one. Else return -1.
        /// </summary>
        /// <returns> -1  there is no discernable key touched
        ///            0-11 the key that has been touched.
        ///            The number corresponds to the number printed on the board.
        /// </returns>
        public int GetKey() {
            int key = NO_EVENT;
            int status = (readByte(1) << 8 | readByte(0));
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
        public void configure() {
            int bytesSent = 0;
            // Section A
            // This group controls filtering when data is > baseline.
            bytesSent = writeTo(new Byte[] { MHD_R, 0x01 });

            bytesSent = writeTo(new Byte[] { NHD_R, 0x01 });

            bytesSent = writeTo(new Byte[] { NCL_R, 0x00 });

            bytesSent = writeTo(new Byte[] { FDL_R, 0x00 });

            // Section B
            // This group controls filtering when data is < baseline.
            bytesSent = writeTo(new Byte[] { MHD_F, 0x01 });

            bytesSent = writeTo(new Byte[] { NHD_F, 0x01 });

            bytesSent = writeTo(new Byte[] { NCL_F, 0xFF });

            bytesSent = writeTo(new Byte[] { FDL_F, 0x02 });

            // Section C
            // This group sets touch and release thresholds for each electrode
            bytesSent = writeTo(new Byte[] { ELE0_T, TOU_THRESH });

            bytesSent = writeTo(new Byte[] { ELE0_R, REL_THRESH });

            bytesSent = writeTo(new Byte[] { ELE1_T, TOU_THRESH });

            bytesSent = writeTo(new Byte[] { ELE1_R, REL_THRESH });

            bytesSent = writeTo(new Byte[] { ELE2_T, TOU_THRESH });

            bytesSent = writeTo(new Byte[] { ELE2_R, REL_THRESH });

            bytesSent = writeTo(new Byte[] { ELE3_T, TOU_THRESH });

            bytesSent = writeTo(new Byte[] { ELE3_R, REL_THRESH });

            bytesSent = writeTo(new Byte[] { ELE4_T, TOU_THRESH });

            bytesSent = writeTo(new Byte[] { ELE4_R, REL_THRESH });

            bytesSent = writeTo(new Byte[] { ELE5_T, TOU_THRESH });

            bytesSent = writeTo(new Byte[] { ELE5_R, REL_THRESH });

            bytesSent = writeTo(new Byte[] { ELE6_T, TOU_THRESH });

            bytesSent = writeTo(new Byte[] { ELE6_R, REL_THRESH });

            bytesSent = writeTo(new Byte[] { ELE7_T, TOU_THRESH });

            bytesSent = writeTo(new Byte[] { ELE7_R, REL_THRESH });

            bytesSent = writeTo(new Byte[] { ELE8_T, TOU_THRESH });

            bytesSent = writeTo(new Byte[] { ELE8_R, REL_THRESH });

            bytesSent = writeTo(new Byte[] { ELE9_T, TOU_THRESH });

            bytesSent = writeTo(new Byte[] { ELE9_R, REL_THRESH });

            bytesSent = writeTo(new Byte[] { ELE10_T, TOU_THRESH });

            bytesSent = writeTo(new Byte[] { ELE10_R, REL_THRESH });

            bytesSent = writeTo(new Byte[] { ELE11_T, TOU_THRESH });

            bytesSent = writeTo(new Byte[] { ELE11_R, REL_THRESH });

            // Section D
            // Set the Filter Configuration
            // Set ESI2

            bytesSent = writeTo(new Byte[] { FIL_CFG, 0x04 });

            // Section E
            // Electrode Configuration
            // Enable 6 Electrodes and set to run mode
            // Set ELE_CFG to 0x00 to return to standby mode

            bytesSent = writeTo(new Byte[] { ELE_CFG, 0x0C });    // Enables all 12 Electrodes        
        }
    }
}

