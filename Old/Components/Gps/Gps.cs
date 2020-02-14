using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Uart;

namespace Gps {
    public class Gps {
        private GpioPin powerControl;        
        private UartController serial;
        private TimeSpan lastPositionReceived;
        private bool enabled = false;

        private PositionReceivedHandler onPositionReceived;

        private NmeaSentenceReceivedHandler onNmeaSentenceReceived;

        private InvalidPositionReceivedHandler onInvalidPositionReceived;

        /// <summary>Represents the delegate that is used to handle the PositionReceived event</summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void PositionReceivedHandler(Gps sender, Position e);

        /// <summary>Represents the delegate that is used to handle the NMEASentenceReceived event</summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void NmeaSentenceReceivedHandler(Gps sender, string e);

        /// <summary>Represents the delegate that is used to handle the InvalidPositionReceived event</summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void InvalidPositionReceivedHandler(Gps sender, EventArgs e);

        /// <summary>Raised when a valid position is received.</summary>
        public event PositionReceivedHandler PositionReceived;

        /// <summary>Raised when an NMEA sentence is received. This is for advanced users who want to parse the NMEA sentences themselves.</summary>
        public event NmeaSentenceReceivedHandler NmeaSentenceReceived;

        /// <summary>Raised when an invalid position is received.</summary>
        public event InvalidPositionReceivedHandler InvalidPositionReceived;

        /// <summary>Enables or disables the GPS.</summary>
		public bool Enabled {
            get => this.powerControl.Read() == GpioPinValue.Low;

            set {
                this.powerControl.Write(value == false ? GpioPinValue.High : GpioPinValue.Low);

                if (!value) 
                    this.serial.Disable();
                
                if (value)
                    this.serial.Enable();

                this.enabled = value;
            }
        }

        /// <summary>The last valid position received.</summary>
		public Position LastPosition { get; private set; }

        /// <summary>The time elapsed since the last position was received.</summary>
        public TimeSpan LastValidPositionAge {
            get {
                if (this.lastPositionReceived == TimeSpan.MinValue)
                    return TimeSpan.MaxValue;

                return TimeSpan.FromTicks(DateTime.Now.Ticks) - this.lastPositionReceived;
            }
        }

        /// <summary>Constructs a new instance.</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public Gps(UartController serial, int powerControl) {

            this.serial = serial;
            this.powerControl = GpioController.GetDefault().OpenPin(powerControl);            

            this.lastPositionReceived = TimeSpan.MinValue;
            this.LastPosition = null;


            serial.SetActiveSettings(9600, 8, UartParity.None, UartStopBitCount.One, UartHandshake.None);            

            new Thread(new ThreadStart(this.ReadLineProcess)).Start();            
        }

        private void OnLineReceived(string line) {
            try {
                if (line != null && line.Length > 0)
                    this.OnNmeaSentenceReceived(this, line);

                if (line.Substring(0, 7) != "$GPRMC,")
                    return;

                var tokens = line.Split(',');
                if (tokens.Length != 13) {
                    this.DebugPrint("RMC NMEA line does not have 13 tokens, ignoring");

                    return;
                }

                if (tokens[2] != "A") {
                    this.OnInvalidPositionReceived(this, null);                    
                }

                var timeRawDouble = double.Parse(tokens[1]);

                var timeRaw = (int)timeRawDouble;
                var hours = timeRaw / 10000;
                var minutes = (timeRaw / 100) % 100;
                var seconds = timeRaw % 100;
                var milliseconds = (int)((timeRawDouble - timeRaw) * 1000.0);
                var dateRaw = int.Parse(tokens[9]);
                var days = dateRaw / 10000;
                var months = (dateRaw / 100) % 100;
                var years = 2000 + (dateRaw % 100);

                var position = new Position {
                    FixTimeUtc = new DateTime(years, months, days, hours, minutes, seconds, milliseconds),
                    LatitudeString = tokens[3] + " " + tokens[4],
                    LongitudeString = tokens[5] + " " + tokens[6]
                };

                var latitudeRaw = double.Parse(tokens[3]);
                var latitudeDegreesRaw = ((int)latitudeRaw) / 100;
                var latitudeMinutesRaw = latitudeRaw - (latitudeDegreesRaw * 100);
                position.Latitude = latitudeDegreesRaw + (latitudeMinutesRaw / 60.0);

                if (tokens[4] == "S")
                    position.Latitude = -position.Latitude;

                var longitudeRaw = double.Parse(tokens[5]);
                var longitudeDegreesRaw = ((int)longitudeRaw) / 100;
                var longitudeMinutesRaw = longitudeRaw - (longitudeDegreesRaw * 100);
                position.Longitude = longitudeDegreesRaw + (longitudeMinutesRaw / 60.0);

                if (tokens[6] == "W")
                    position.Longitude = -position.Longitude;

                position.SpeedKnots = 0;
                if (tokens[7] != "")
                    position.SpeedKnots = double.Parse(tokens[7]);

                position.CourseDegrees = 0;
                if (tokens[8] != "")
                    position.CourseDegrees = double.Parse(tokens[8]);

                this.lastPositionReceived = TimeSpan.FromTicks(DateTime.Now.Ticks);
                this.LastPosition = position;
                this.OnPositionReceived(this, position);
            }
            catch {
                this.DebugPrint("Error parsing RMC NMEA message");
            }
        }

        private void OnPositionReceived(Gps sender, Position e) {
            if (this.onPositionReceived == null)
                this.onPositionReceived = this.OnPositionReceived;

            this.PositionReceived?.Invoke(sender, e);
        }

        private void OnNmeaSentenceReceived(Gps sender, string e) {
            if (this.onNmeaSentenceReceived == null)
                this.onNmeaSentenceReceived = this.OnNmeaSentenceReceived;

            this.NmeaSentenceReceived?.Invoke(sender, e);
        }

        private void OnInvalidPositionReceived(Gps sender, EventArgs e) {
            if (this.onInvalidPositionReceived == null)
                this.onInvalidPositionReceived = this.OnInvalidPositionReceived;

            this.InvalidPositionReceived?.Invoke(sender, e);
        }

        /// <summary>Represents a GPS position.</summary>
		public class Position {

            /// <summary>The latitude.</summary>
            public double Latitude { get; set; }

            /// <summary>The longitude.</summary>
            public double Longitude { get; set; }

            /// <summary>A string representing the latitude, in the format ddmm.mmmm H, where dd = degrees, mm.mmm = minutes and fractional minutes, and H = hemisphere (N/S).</summary>
            public string LatitudeString { get; set; }

            /// <summary>A string representing the longitude, in the format ddmm.mmmm H, where dd = degrees, mm.mmm = minutes and fractional minutes, and H = hemisphere (E/W).</summary>
            public string LongitudeString { get; set; }

            /// <summary>Speed over the ground in knots.</summary>
            public double SpeedKnots { get; set; }

            /// <summary>Course over the ground in degrees.</summary>
            public double CourseDegrees { get; set; }

            /// <summary>The Universal Coordinated Time (UTC) time of the fix.</summary>
            public DateTime FixTimeUtc { get; set; }

            /// <summary>Provides a formatted string for this Position.</summary>
            /// <returns>The formatted string.</returns>
            public override string ToString() => "Lat " + this.Latitude + ", Long " + this.Longitude + ", Speed " + this.SpeedKnots + ", Course " + this.CourseDegrees + ", FixTime " + this.FixTimeUtc.ToString();
        }

        private string lineReceivedEventDelimiter = "\n";

        public string LineReceivedEventDelimiter {
            get => this.lineReceivedEventDelimiter;

            set {
                if (value == null) {
                    throw new ArgumentNullException();
                }

                if (value.Length == 0) {
                    throw new ArgumentException();
                }

                this.lineReceivedEventDelimiter = value;
            }
        }

        private void ReadLineProcess() {
            var outstandingText = string.Empty;
            var buf = new byte[1];
            
            while (true) {
                try {
                    if (!this.enabled) {
                        Thread.Sleep(100);
                        continue;
                    }

                    var res = -1;
                    lock (this.serial)//Lock to avoid a close() call between isOpen and read
                    {
                        if (this.enabled)//Check it did not get closed
                        {
                            res = this.serial.Read(buf, 0, 1);
                        }
                        else {
                            continue; //Closed 
                        }
                    }
                    if (res <= 0) {
                        continue;
                    }

                    var charData = System.Text.Encoding.UTF8.GetChars(buf);
                    var s = new string(charData);
                    outstandingText = outstandingText + s;

                    if (outstandingText.Length >= this.LineReceivedEventDelimiter.Length) {
                        if (
                            outstandingText.Substring(outstandingText.Length -
                                                      this.LineReceivedEventDelimiter.Length).Equals(
                                                          this.LineReceivedEventDelimiter)) {
                            // Chop end pattern off.
                            outstandingText = outstandingText.Substring(0,
                                                                        outstandingText.Length -
                                                                        this.LineReceivedEventDelimiter.Length);

                            this.OnLineReceived(outstandingText);
                            outstandingText = string.Empty;
                        }
                    }

                }
                catch {
                    this.DebugPrint("Exception parsing serial line");
                }
            }
        }

        private void DebugPrint(string s) => System.Diagnostics.Debug.WriteLine(s);
    }
}
