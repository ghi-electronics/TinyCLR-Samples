using GHIElectronics.TinyCLR.Native;
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;

namespace EcstaticLed {
    public class AviDecoder {
        private Stream stream;
        private Queue queue;
        private bool decoding;

        private byte[] buffer;

        public int MaxFrameCache { get; set; } = 200;

        public class HeaderInfo {
            public int TimeBetweenFrames { get; internal set; }
            public int Width { get; internal set; }
            public int Height { get; internal set; }
            public int SuggestedBufferSize { get; internal set; }
            public int TotalFrames { get; internal set; }

        }

        public HeaderInfo headerInfo;

        public AviDecoder(Stream stream) {
            this.stream = stream;

            this.queue = new Queue();

            this.decoding = true;

            this.headerInfo = new HeaderInfo();
        }

        ~AviDecoder() => this.decoding = false;

        public Bitmap GetBimap() {
            lock (this.queue) {
                if (this.queue.Count > 0) {
                    return (Bitmap)this.queue.Dequeue();
                }

                return null;
            }
        }

        public void Run() => new Thread(this.ThreadStart).Start();

        private void ThreadStart() {

            var decodedHeader = false;
            var startFrame = 72;

            while (this.decoding) {

                int i;
                if (decodedHeader == false) {
                    var header = new byte[72];

                    this.stream.Read(header, 0, header.Length);

                    var riff = System.Text.Encoding.UTF8.GetString(header, 0, 4);
                    var type = System.Text.Encoding.UTF8.GetString(header, 8, 4);

                    if (riff.CompareTo("RIFF") == 0 || type.CompareTo("AVI ") == 0) {

                        i = 0x20; // fps

                        this.headerInfo.TimeBetweenFrames = (header[i] | (header[i + 1] << 8) | (header[i + 2] << 16) | (header[i + 3] << 24)) / 1000;

                        i += 4 * 4;

                        this.headerInfo.TotalFrames = header[i] | (header[i + 1] << 8) | (header[i + 2] << 16) | (header[i + 3] << 24);

                        i += 3 * 4;

                        this.headerInfo.SuggestedBufferSize = header[i] | (header[i + 1] << 8) | (header[i + 2] << 16) | (header[i + 3] << 24);


                        i += 4;

                        this.headerInfo.Width = header[i] | (header[i + 1] << 8) | (header[i + 2] << 16) | (header[i + 3] << 24);

                        i += 4;

                        this.headerInfo.Height = header[i] | (header[i + 1] << 8) | (header[i + 2] << 16) | (header[i + 3] << 24);

                        i += 4;
                    }
                    else {
                        throw new ArgumentException("Not support.");
                    }

                    decodedHeader = true;

                    i = 0;
                }

_calculate_cache_size: // Calculate need buffer

                if (this.buffer == null) {
                    while (this.headerInfo.SuggestedBufferSize > (Memory.ManagedMemory.FreeBytes / 2)) {
                        this.headerInfo.SuggestedBufferSize /= 2;

                        goto _calculate_cache_size;

                    }

                    if (this.headerInfo.SuggestedBufferSize > (this.stream.Length - startFrame))
                        this.headerInfo.SuggestedBufferSize = (int)this.stream.Length - startFrame;

                    this.buffer = new byte[this.headerInfo.SuggestedBufferSize];
                }

do_repeat:

                var block = this.buffer.Length / 4096;
                var remain = this.buffer.Length % 4096;
                var blockIdx = 0;

                while (block > 0) {
                    if (this.stream.Read(this.buffer, blockIdx, 4096) < 4096) {
                        this.stream.Seek(startFrame, SeekOrigin.Begin);
                        goto do_repeat;
                    }
                    blockIdx += 4096;
                    block--;
                    Thread.Sleep(1);
                }
                if (remain > 0) {
                    if (this.stream.Read(this.buffer, blockIdx, remain) < remain) {
                        this.stream.Seek(startFrame, SeekOrigin.Begin);
                        goto do_repeat;
                    }
                }

                for (i = 0; i < this.buffer.Length - 3; i++) {
                    // Decode image
                    {
                        if (this.buffer[i + 0] == (byte)'0'
                        && this.buffer[i + 1] == (byte)'0'
                        && this.buffer[i + 2] == (byte)'d'
                        && this.buffer[i + 3] == (byte)'c') {
                            i += 4;

                            var jpegLength = (this.buffer[i] | (this.buffer[i + 1] << 8) | (this.buffer[i + 2] << 16) | (this.buffer[i + 3] << 24));
                            i += 4;

                            if (i + jpegLength > this.buffer.Length)
                                break;

                            if (jpegLength > 16) // 0x10 is for information
                            {
                                try {
                                    var bitmap = new Bitmap(this.buffer, i, jpegLength, BitmapImageType.Jpeg);

                                    lock (this.queue) {
                                        this.queue.Enqueue(bitmap);
                                    }
                                }
                                catch {

                                }

                                if (this.queue.Count > this.MaxFrameCache) {
                                    while (this.queue.Count > this.MaxFrameCache / 2) {
                                        Thread.Sleep(10);
                                    }
                                }
                                else {
                                    Thread.Sleep(1);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
