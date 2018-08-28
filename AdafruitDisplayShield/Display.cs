using System;
using System.Collections;
using System.Text;
using System.Threading;

using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;


// https://www.adafruit.com/product/802
// schematics https://learn.adafruit.com/assets/35824
// The displlay initilizations code is based on the Adafruit's drivers 

namespace Adafruit.Display18 {
    /// <summary>
    /// Represents a color made up of red, green, and blue.
    /// </summary>
    public class Color {
        /// <summary>
        /// The amount of red.
        /// </summary>
        public byte R { get; set; }

        /// <summary>
        /// The amount of green.
        /// </summary>
        public byte G { get; set; }

        /// <summary>
        /// The amount of blue.
        /// </summary>
        public byte B { get; set; }

        /// <summary>
        /// The color in 565 format.
        /// </summary>
        public ushort As565 {
            get {
                return (ushort)(((this.R & 0x1F) << 11) | ((this.G & 0x3F) << 5) | (this.B & 0x1F));
            }
        }

        /// <summary>
        /// Constructs a new instance with the given levels.
        /// </summary>
        public Color()
            : this(0, 0, 0) {

        }

        /// <summary>
        /// Constructs a new instance with the given levels.
        /// </summary>
        /// <param name="red">The amount of red.</param>
        /// <param name="blue">The amount of blue.</param>
        public Color(byte red, byte green, byte blue) {
            this.R = red;
            this.G = green;
            this.B = blue;
        }

        /// <summary>
        /// A predefined color for black.
        /// </summary>
        public static Color Black = new Color(0, 0, 0);
        /// <summary>
        /// A predefined color for white.
        /// </summary>
        public static Color White = new Color(255, 255, 255);
        /// <summary>
        /// A predefined color for red.
        /// </summary>
        public static Color Red = new Color(255, 0, 0);
        /// <summary>
        /// A predefined color for green.
        /// </summary>
        public static Color Green = new Color(0, 255, 0);
        /// <summary>
        /// A predefined color for blue.
        /// </summary>
        public static Color Blue = new Color(0, 0, 255);
        /// <summary>
        /// A predefined color for yellow.
        /// </summary>
        public static Color Yellow = new Color(255, 255, 0);
        /// <summary>
        /// A predefined color for cyan.
        /// </summary>
        public static Color Cyan = new Color(0, 255, 255);
        /// <summary>
        /// A predefined color for magenta.
        /// </summary>
        public static Color Magenta = new Color(255, 0, 255);
    }



    public class Display {
        //private   SPI spi;
        private   SpiDevice spi;

        //private   OutputPort controlPin;
        //private   OutputPort resetPin;
        //private   OutputPort backlightPin;

        private   GpioPin controlPin;
        //private   GpioPin resetPin;
        //private   GpioPin backlightPin;

        private   byte[] buffer1;
        private   byte[] buffer2;
        private   byte[] buffer4;
        private   byte[] clearBuffer;
        private   byte[] characterBuffer1;
        private   byte[] characterBuffer2;
        private   byte[] characterBuffer4;

        private const byte ST7735_MADCTL = 0x36;
        private const byte MADCTL_MY = 0x80;
        private const byte MADCTL_MX = 0x40;
        private const byte MADCTL_MV = 0x20;
        private const byte MADCTL_BGR = 0x08;

        /// <summary>
        /// The width of the display in pixels.
        /// </summary>
        public const int Width = 160;

        /// <summary>
        /// The height of the display in pixels.
        /// </summary>
        public const int Height = 128;

        public Display(int CtrlPin, int ChipSelect, string SpiBus) {
            buffer1 = new byte[1];
            buffer2 = new byte[2];
            buffer4 = new byte[4];
            clearBuffer = new byte[160 * 2 * 16];
            characterBuffer1 = new byte[80];
            characterBuffer2 = new byte[320];
            characterBuffer4 = new byte[1280];
            GpioController GPIO = GpioController.GetDefault();
            controlPin = GPIO.OpenPin(CtrlPin);

            controlPin.SetDriveMode(GpioPinDriveMode.Output);

            var settings = new SpiConnectionSettings(ChipSelect);
            settings.Mode = SpiMode.Mode3;
            settings.ClockFrequency = 4000;
            settings.DataBitLength = 8;
            //var aqs = SpiDevice.GetDeviceSelector("SPI1");
            spi = SpiDevice.FromId(SpiBus, settings);

            
            //Reset();
            Init();
            Init();// the display only seem to work when init twice!

            Clear();
        }
        private void Init() {

            WriteCommand(0x11); //Sleep exit 
            Thread.Sleep(200);

            // ST7735R Frame Rate
            WriteCommand(0xB1);
            WriteData(0x01); WriteData(0x2C); WriteData(0x2D);
            WriteCommand(0xB2);
            WriteData(0x01); WriteData(0x2C); WriteData(0x2D);
            WriteCommand(0xB3);
            WriteData(0x01); WriteData(0x2C); WriteData(0x2D);
            WriteData(0x01); WriteData(0x2C); WriteData(0x2D);

            WriteCommand(0xB4); // Column inversion 
            WriteData(0x07);

            // ST7735R Power Sequence
            WriteCommand(0xC0);
            WriteData(0xA2); WriteData(0x02); WriteData(0x84);
            WriteCommand(0xC1); WriteData(0xC5);
            WriteCommand(0xC2);
            WriteData(0x0A); WriteData(0x00);
            WriteCommand(0xC3);
            WriteData(0x8A); WriteData(0x2A);
            WriteCommand(0xC4);
            WriteData(0x8A); WriteData(0xEE);

            WriteCommand(0xC5); // VCOM 
            WriteData(0x0E);

            WriteCommand(0x36); // MX, MY, RGB mode
            WriteData(MADCTL_MX | MADCTL_MY | MADCTL_BGR);

            // ST7735R Gamma Sequence
            WriteCommand(0xe0);
            WriteData(0x0f); WriteData(0x1a);
            WriteData(0x0f); WriteData(0x18);
            WriteData(0x2f); WriteData(0x28);
            WriteData(0x20); WriteData(0x22);
            WriteData(0x1f); WriteData(0x1b);
            WriteData(0x23); WriteData(0x37); WriteData(0x00);

            WriteData(0x07);
            WriteData(0x02); WriteData(0x10);
            WriteCommand(0xe1);
            WriteData(0x0f); WriteData(0x1b);
            WriteData(0x0f); WriteData(0x17);
            WriteData(0x33); WriteData(0x2c);
            WriteData(0x29); WriteData(0x2e);
            WriteData(0x30); WriteData(0x30);
            WriteData(0x39); WriteData(0x3f);
            WriteData(0x00); WriteData(0x07);
            WriteData(0x03); WriteData(0x10);

            WriteCommand(0x2a);
            WriteData(0x00); WriteData(0x00);
            WriteData(0x00); WriteData(0x7f);
            WriteCommand(0x2b);
            WriteData(0x00); WriteData(0x00);
            WriteData(0x00); WriteData(0x9f);

            WriteCommand(0xF0); //Enable test command  
            WriteData(0x01);
            WriteCommand(0xF6); //Disable ram power save mode 
            WriteData(0x00);

            WriteCommand(0x3A); //65k mode 
            WriteData(0x05);

            // Rotate
            WriteCommand(ST7735_MADCTL);
            WriteData(MADCTL_MV | MADCTL_MY);

            WriteCommand(0x29); //Display on
            Thread.Sleep(50);

           
        }
        private   void WriteData(byte[] data) {
            controlPin.Write(GpioPinValue.High);
            spi.Write(data);
            //Thread.Sleep(0);
        }

        private   void WriteCommand(byte command) {
            buffer1[0] = command;
            controlPin.Write(GpioPinValue.Low);
            spi.Write(buffer1);
            //Thread.Sleep(0);
        }

        private   void WriteData(byte data) {
            buffer1[0] = data;
            controlPin.Write(GpioPinValue.High);
            spi.Write(buffer1);
            //Thread.Sleep(0);
        }

        private   void Reset() {
            //resetPin.Write(false);
            Thread.Sleep(300);
            //resetPin.Write(true);
            Thread.Sleep(1000);
        }

        private   void SetClip(int x, int y, int width, int height) {
            WriteCommand(0x2A);

            controlPin.Write(GpioPinValue.High);
            buffer4[1] = (byte)x;
            buffer4[3] = (byte)(x + width - 1);
            spi.Write(buffer4);

            WriteCommand(0x2B);
            controlPin.Write(GpioPinValue.High);
            buffer4[1] = (byte)y;
            buffer4[3] = (byte)(y + height - 1);
            spi.Write(buffer4);
        }

        /// <summary>
        /// Clears the Display.
        /// </summary>
        public void Clear() {
            SetClip(0, 0, 160, 128);
            WriteCommand(0x2C);

            for (var i = 0; i < 128 / 16; i++)
                WriteData(clearBuffer);
        }

        /// <summary>
        /// Draws an image.
        /// </summary>
        /// <param name="data">The image as a byte array.</param>
        public   void DrawImage(byte[] data) {
            if (data == null) throw new ArgumentNullException("data");
            if (data.Length == 0) throw new ArgumentException("data.Length must not be zero.", "data");

            WriteCommand(0x2C);
            WriteData(data);
        }

        /// <summary>
        /// Draws an image at the given location.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="image">The image to draw.</param>
        /*public   void DrawImage(int x, int y, Image image)
        {
            if (image == null) throw new ArgumentNullException("image");
            if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
            if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");

            SetClip(x, y, image.Width, image.Height);
            DrawImage(image.Pixels);
        }*/

        /// <summary>
        /// Draws a filled rectangle.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <param name="color">The color to draw.</param>
        public   void DrawFilledRectangle(int x, int y, int width, int height, Color color) {
            if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
            if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");
            if (width < 0) throw new ArgumentOutOfRangeException("width", "width must not be negative.");
            if (height < 0) throw new ArgumentOutOfRangeException("height", "height must not be negative.");

            SetClip(x, y, width, height);

            var data = new byte[width * height * 2];
            for (var i = 0; i < data.Length; i += 2) {
                data[i] = (byte)((color.As565 >> 8) & 0xFF);
                data[i + 1] = (byte)((color.As565 >> 0) & 0xFF);
            }

            DrawImage(data);
        }

        /// <summary>
        /// Turns the backlight on.
        /// </summary>
        public   void TurnOn() {
            //backlightPin.Write(true);
        }

        /// <summary>
        /// Turns the backlight off.
        /// </summary>
        public   void TurnOff() {
            // backlightPin.Write(false);
        }

        /// <summary>
        /// Draws a pixel.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="color">The color to draw.</param>
        public   void SetPixel(int x, int y, Color color) {
            if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
            if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");

            SetClip(x, y, 1, 1);

            buffer2[0] = (byte)(color.As565 >> 8);
            buffer2[1] = (byte)(color.As565 >> 0);

            DrawImage(buffer2);
        }

        /// <summary>
        /// Draws a line.
        /// </summary>
        /// <param name="x">The x coordinate to start drawing at.</param>
        /// <param name="y">The y coordinate to start drawing at.</param>
        /// <param name="x1">The ending x coordinate.</param>
        /// <param name="y1">The ending y coordinate.</param>
        /// <param name="color">The color to draw.</param>
        public   void DrawLine(int x0, int y0, int x1, int y1, Color color) {
            if (x0 < 0) throw new ArgumentOutOfRangeException("x0", "x0 must not be negative.");
            if (y0 < 0) throw new ArgumentOutOfRangeException("y0", "y0 must not be negative.");
            if (x1 < 0) throw new ArgumentOutOfRangeException("x1", "x1 must not be negative.");
            if (y1 < 0) throw new ArgumentOutOfRangeException("y1", "y1 must not be negative.");

            var steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            int t, dX, dY, yStep, error;

            if (steep) {
                t = x0;
                x0 = y0;
                y0 = t;
                t = x1;
                x1 = y1;
                y1 = t;
            }

            if (x0 > x1) {
                t = x0;
                x0 = x1;
                x1 = t;

                t = y0;
                y0 = y1;
                y1 = t;
            }

            dX = x1 - x0;
            dY = System.Math.Abs(y1 - y0);

            error = (dX / 2);

            if (y0 < y1) {
                yStep = 1;
            }
            else {
                yStep = -1;
            }

            for (; x0 < x1; x0++) {
                if (steep) {
                    SetPixel(y0, x0, color);
                }
                else {
                    SetPixel(x0, y0, color);
                }

                error -= dY;

                if (error < 0) {
                    y0 += (byte)yStep;
                    error += dX;
                }
            }
        }

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="r">The radius of the circle.</param>
        /// <param name="color">The color to draw.</param>
        public   void DrawCircle(int x, int y, int r, Color color) {
            if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
            if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");
            if (r <= 0) throw new ArgumentOutOfRangeException("radius", "radius must be positive.");

            int f = 1 - r;
            int ddFX = 1;
            int ddFY = -2 * r;
            int dX = 0;
            int dY = r;

            SetPixel(x, y + r, color);
            SetPixel(x, y - r, color);
            SetPixel(x + r, y, color);
            SetPixel(x - r, y, color);

            while (dX < dY) {
                if (f >= 0) {
                    dY--;
                    ddFY += 2;
                    f += ddFY;
                }

                dX++;
                ddFX += 2;
                f += ddFX;

                SetPixel(x + dX, y + dY, color);
                SetPixel(x - dX, y + dY, color);
                SetPixel(x + dX, y - dY, color);
                SetPixel(x - dX, y - dY, color);

                SetPixel(x + dY, y + dX, color);
                SetPixel(x - dY, y + dX, color);
                SetPixel(x + dY, y - dX, color);
                SetPixel(x - dY, y - dX, color);
            }
        }

        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <param name="color">The color to use.</param>
        public   void DrawRectangle(int x, int y, int width, int height, Color color) {
            if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
            if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");
            if (width < 0) throw new ArgumentOutOfRangeException("width", "width must not be negative.");
            if (height < 0) throw new ArgumentOutOfRangeException("height", "height must not be negative.");

            for (var i = x; i < x + width; i++) {
                SetPixel(i, y, color);
                SetPixel(i, y + height - 1, color);
            }

            for (var i = y; i < y + height; i++) {
                SetPixel(x, i, color);
                SetPixel(x + width - 1, i, color);
            }
        }

         byte[] font = new byte[95 * 5] {
            0x00, 0x00, 0x00, 0x00, 0x00, /* Space	0x20 */
            0x00, 0x00, 0x4f, 0x00, 0x00, /* ! */
            0x00, 0x07, 0x00, 0x07, 0x00, /* " */
            0x14, 0x7f, 0x14, 0x7f, 0x14, /* # */
            0x24, 0x2a, 0x7f, 0x2a, 0x12, /* $ */
            0x23, 0x13, 0x08, 0x64, 0x62, /* % */
            0x36, 0x49, 0x55, 0x22, 0x20, /* & */
            0x00, 0x05, 0x03, 0x00, 0x00, /* ' */
            0x00, 0x1c, 0x22, 0x41, 0x00, /* ( */
            0x00, 0x41, 0x22, 0x1c, 0x00, /* ) */
            0x14, 0x08, 0x3e, 0x08, 0x14, /* // */
            0x08, 0x08, 0x3e, 0x08, 0x08, /* + */
            0x50, 0x30, 0x00, 0x00, 0x00, /* , */
            0x08, 0x08, 0x08, 0x08, 0x08, /* - */
            0x00, 0x60, 0x60, 0x00, 0x00, /* . */
            0x20, 0x10, 0x08, 0x04, 0x02, /* / */
            0x3e, 0x51, 0x49, 0x45, 0x3e, /* 0		0x30 */
            0x00, 0x42, 0x7f, 0x40, 0x00, /* 1 */
            0x42, 0x61, 0x51, 0x49, 0x46, /* 2 */
            0x21, 0x41, 0x45, 0x4b, 0x31, /* 3 */
            0x18, 0x14, 0x12, 0x7f, 0x10, /* 4 */
            0x27, 0x45, 0x45, 0x45, 0x39, /* 5 */
            0x3c, 0x4a, 0x49, 0x49, 0x30, /* 6 */
            0x01, 0x71, 0x09, 0x05, 0x03, /* 7 */
            0x36, 0x49, 0x49, 0x49, 0x36, /* 8 */
            0x06, 0x49, 0x49, 0x29, 0x1e, /* 9 */
            0x00, 0x36, 0x36, 0x00, 0x00, /* : */
            0x00, 0x56, 0x36, 0x00, 0x00, /* ; */
            0x08, 0x14, 0x22, 0x41, 0x00, /* < */
            0x14, 0x14, 0x14, 0x14, 0x14, /* = */
            0x00, 0x41, 0x22, 0x14, 0x08, /* > */
            0x02, 0x01, 0x51, 0x09, 0x06, /* ? */
            0x3e, 0x41, 0x5d, 0x55, 0x1e, /* @		0x40 */
            0x7e, 0x11, 0x11, 0x11, 0x7e, /* A */
            0x7f, 0x49, 0x49, 0x49, 0x36, /* B */
            0x3e, 0x41, 0x41, 0x41, 0x22, /* C */
            0x7f, 0x41, 0x41, 0x22, 0x1c, /* D */
            0x7f, 0x49, 0x49, 0x49, 0x41, /* E */
            0x7f, 0x09, 0x09, 0x09, 0x01, /* F */
            0x3e, 0x41, 0x49, 0x49, 0x7a, /* G */
            0x7f, 0x08, 0x08, 0x08, 0x7f, /* H */
            0x00, 0x41, 0x7f, 0x41, 0x00, /* I */
            0x20, 0x40, 0x41, 0x3f, 0x01, /* J */
            0x7f, 0x08, 0x14, 0x22, 0x41, /* K */
            0x7f, 0x40, 0x40, 0x40, 0x40, /* L */
            0x7f, 0x02, 0x0c, 0x02, 0x7f, /* M */
            0x7f, 0x04, 0x08, 0x10, 0x7f, /* N */
            0x3e, 0x41, 0x41, 0x41, 0x3e, /* O */
            0x7f, 0x09, 0x09, 0x09, 0x06, /* P		0x50 */
            0x3e, 0x41, 0x51, 0x21, 0x5e, /* Q */
            0x7f, 0x09, 0x19, 0x29, 0x46, /* R */
            0x26, 0x49, 0x49, 0x49, 0x32, /* S */
            0x01, 0x01, 0x7f, 0x01, 0x01, /* T */
            0x3f, 0x40, 0x40, 0x40, 0x3f, /* U */
            0x1f, 0x20, 0x40, 0x20, 0x1f, /* V */
            0x3f, 0x40, 0x38, 0x40, 0x3f, /* W */
            0x63, 0x14, 0x08, 0x14, 0x63, /* X */
            0x07, 0x08, 0x70, 0x08, 0x07, /* Y */
            0x61, 0x51, 0x49, 0x45, 0x43, /* Z */
            0x00, 0x7f, 0x41, 0x41, 0x00, /* [ */
            0x02, 0x04, 0x08, 0x10, 0x20, /* \ */
            0x00, 0x41, 0x41, 0x7f, 0x00, /* ] */
            0x04, 0x02, 0x01, 0x02, 0x04, /* ^ */
            0x40, 0x40, 0x40, 0x40, 0x40, /* _ */
            0x00, 0x00, 0x03, 0x05, 0x00, /* `		0x60 */
            0x20, 0x54, 0x54, 0x54, 0x78, /* a */
            0x7F, 0x44, 0x44, 0x44, 0x38, /* b */
            0x38, 0x44, 0x44, 0x44, 0x44, /* c */
            0x38, 0x44, 0x44, 0x44, 0x7f, /* d */
            0x38, 0x54, 0x54, 0x54, 0x18, /* e */
            0x04, 0x04, 0x7e, 0x05, 0x05, /* f */
            0x08, 0x54, 0x54, 0x54, 0x3c, /* g */
            0x7f, 0x08, 0x04, 0x04, 0x78, /* h */
            0x00, 0x44, 0x7d, 0x40, 0x00, /* i */
            0x20, 0x40, 0x44, 0x3d, 0x00, /* j */
            0x7f, 0x10, 0x28, 0x44, 0x00, /* k */
            0x00, 0x41, 0x7f, 0x40, 0x00, /* l */
            0x7c, 0x04, 0x7c, 0x04, 0x78, /* m */
            0x7c, 0x08, 0x04, 0x04, 0x78, /* n */
            0x38, 0x44, 0x44, 0x44, 0x38, /* o */
            0x7c, 0x14, 0x14, 0x14, 0x08, /* p		0x70 */
            0x08, 0x14, 0x14, 0x14, 0x7c, /* q */
            0x7c, 0x08, 0x04, 0x04, 0x00, /* r */
            0x48, 0x54, 0x54, 0x54, 0x24, /* s */
            0x04, 0x04, 0x3f, 0x44, 0x44, /* t */
            0x3c, 0x40, 0x40, 0x20, 0x7c, /* u */
            0x1c, 0x20, 0x40, 0x20, 0x1c, /* v */
            0x3c, 0x40, 0x30, 0x40, 0x3c, /* w */
            0x44, 0x28, 0x10, 0x28, 0x44, /* x */
            0x0c, 0x50, 0x50, 0x50, 0x3c, /* y */
            0x44, 0x64, 0x54, 0x4c, 0x44, /* z */
            0x08, 0x36, 0x41, 0x41, 0x00, /* { */
            0x00, 0x00, 0x77, 0x00, 0x00, /* | */
            0x00, 0x41, 0x41, 0x36, 0x08, /* } */
            0x08, 0x08, 0x2a, 0x1c, 0x08  /* ~ */
        };

        private   void DrawLetter(int x, int y, char letter, Color color, int scaleFactor) {
            if (letter < 32) return;
            var index = 5 * (letter - 32);
            var upper = (byte)(color.As565 >> 8);
            var lower = (byte)(color.As565 >> 0);
            var characterBuffer = scaleFactor == 1 ? characterBuffer1 : (scaleFactor == 2 ? characterBuffer2 : characterBuffer4);

            var i = 0;

            for (var j = 1; j <= 64; j *= 2) {
                for (var k = 0; k < scaleFactor; k++) {
                    for (var l = 0; l < 5; l++) {
                        for (var m = 0; m < scaleFactor; m++) {
                            var show = (font[index + l] & j) != 0;

                            characterBuffer[i++] = show ? upper : (byte)0x00;
                            characterBuffer[i++] = show ? lower : (byte)0x00;
                        }
                    }
                }
            }

            SetClip(x, y, 5 * scaleFactor, 8 * scaleFactor);
            DrawImage(characterBuffer);
        }

        /// <summary>
        /// Draws a letter at the given location.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="letter">The letter to draw.</param>
        /// <param name="color">The color to use.</param>
        public   void DrawLetter(int x, int y, char letter, Color color) {
            if (letter > 126 || letter < 32) throw new ArgumentOutOfRangeException("letter", "This letter cannot be drawn.");
            if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
            if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");

            DrawLetter(x, y, letter, color, 1);
        }

        /// <summary>
        /// Draws a large letter at the given location.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="letter">The letter to draw.</param>
        /// <param name="color">The color to use.</param>
        public   void DrawLargeLetter(int x, int y, char letter, Color color) {
            if (letter > 126 || letter < 32) throw new ArgumentOutOfRangeException("letter", "This letter cannot be drawn.");
            if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
            if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");

            DrawLetter(x, y, letter, color, 2);
        }

        /// <summary>
        /// Draws an extra large letter at the given location.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="letter">The letter to draw.</param>
        /// <param name="color">The color to use.</param>
        public   void DrawExtraLargeLetter(int x, int y, char letter, Color color) {
            if (letter > 126 || letter < 32) throw new ArgumentOutOfRangeException("letter", "This letter cannot be drawn.");
            if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
            if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");

            DrawLetter(x, y, letter, color, 4);
        }

        /// <summary>
        /// Draws text at the given location.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="text">The string to draw.</param>
        /// <param name="color">The color to use.</param>
        public   void DrawText(int x, int y, string text, Color color) {
            if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
            if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");
            if (text == null) throw new ArgumentNullException("data");

            for (var i = 0; i < text.Length; i++)
                DrawLetter(x + i * 6, y, text[i], color, 1);
        }

        /// <summary>
        /// Draws large text at the given location.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="text">The string to draw.</param>
        /// <param name="color">The color to use.</param>
        public   void DrawLargeText(int x, int y, string text, Color color) {
            if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
            if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");
            if (text == null) throw new ArgumentNullException("data");

            for (var i = 0; i < text.Length; i++)
                DrawLetter(x + i * 6 * 2, y, text[i], color, 2);
        }

        /// <summary>
        /// Draws extra large text at the given location.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="text">The string to draw.</param>
        /// <param name="color">The color to use.</param>
        public   void DrawExtraLargeText(int x, int y, string text, Color color) {
            if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
            if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");
            if (text == null) throw new ArgumentNullException("data");

            for (var i = 0; i < text.Length; i++)
                DrawLetter(x + i * 6 * 4, y, text[i], color, 4);
        }

        /// <summary>
        /// Draws a number at the given location.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="number">The number to draw.</param>
        /// <param name="color">The color to use.</param>
        public   void DrawNumber(int x, int y, double number, Color color) {
            if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
            if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");

            DrawText(x, y, number.ToString("N2"), color);
        }

        /// <summary>
        /// Draws a large number at the given location.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="number">The number to draw.</param>
        /// <param name="color">The color to use.</param>
        public   void DrawLargeNumber(int x, int y, double number, Color color) {
            if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
            if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");

            DrawLargeText(x, y, number.ToString("N2"), color);
        }

        /// <summary>
        /// Draws an extra large number at the given location.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="number">The number to draw.</param>
        /// <param name="color">The color to use.</param>
        public   void DrawExtraLargeNumber(int x, int y, double number, Color color) {
            if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
            if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");

            DrawExtraLargeText(x, y, number.ToString("N2"), color);
        }

        /// <summary>
        /// Draws a number at the given location.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="number">The number to draw.</param>
        /// <param name="color">The color to use.</param>
        public   void DrawNumber(int x, int y, long number, Color color) {
            if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
            if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");

            DrawText(x, y, number.ToString("N0"), color);
        }

        /// <summary>
        /// Draws a large number at the given location.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="number">The number to draw.</param>
        /// <param name="color">The color to use.</param>
        public   void DrawLargeNumber(int x, int y, long number, Color color) {
            if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
            if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");

            DrawLargeText(x, y, number.ToString("N0"), color);
        }

        /// <summary>
        /// Draws an extra large number at the given location.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="number">The number to draw.</param>
        /// <param name="color">The color to use.</param>
        public   void DrawExtraLargeNumber(int x, int y, long number, Color color) {
            if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
            if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");

            DrawExtraLargeText(x, y, number.ToString("N0"), color);
        }
    }
}
