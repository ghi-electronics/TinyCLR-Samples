// https://www.sparkfun.com/products/10587
// VS1053 in MIDI mode, a quality synthesizer! Meaning it will not play MP3 without modifications to the board
// The mode is set by pulling GPIO0 low and GPIO1 high on power up
// The MIDI stream goes in serially. They used D3 with software serial to keep D1 free for arduino loder
// Add a tiny wire from the onboard solder pad labeled MIDI-in to pin D1, which is the proper TX UART pin
// we do not use reset pin

using System;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.Threading;

using GHIElectronics.TinyCLR.Devices.Uart;
using GHIElectronics.TinyCLR.Pins;

namespace GHIElectronics.TinyCLR.Drivers.Shield {
    class MusicalInstrumentShield {
        private UartController uart;
        private byte[] b1 = new byte[1];
        public MusicalInstrumentShield(string uart) {
            this.uart = UartController.FromName(uart);
            this.uart.SetActiveSettings((int)(31250), 8,
                UartParity.None,
                UartStopBitCount.One,
                UartHandshake.None);

            this.uart.Enable();

        }
        public void PlayFancySoundsDemo() {
            //Demo GM2 / Fancy sounds
            //=================================================================
            Debug.WriteLine("Demo Fancy Sounds");
            this.TalkMIDI(0xB0, 0, 0x78); //Bank select drums

            //For this bank 0x78, the instrument does not matter, only the note
            for (var instrument = 30; instrument < 31; instrument++) {

                Debug.WriteLine(" Instrument: " + instrument);

                this.TalkMIDI(0xC0, instrument, 0); //Set instrument number. 0xC0 is a 1 data byte command

                //Play fancy sounds from 'High Q' to 'Open Surdo [EXC 6]'
                for (var note = 27; note < 87; note++) {
                    Debug.WriteLine("N:"+ note);

                    //Note on channel 1 (0x90), some note value (note), middle velocity (0x45):
                    this.NoteOn(0, note, 60);
                    Thread.Sleep(100);

                    //Turn off the note with a given off/release velocity
                    this.NoteOff(0, note, 60);
                    Thread.Sleep(100);
                }

                Thread.Sleep(100); //Delay between instruments
            }
        }
        public void PlayMelodicDemo() {
            //Demo Melodic
            //=================================================================
            Debug.WriteLine("Demo Melodic? Sounds");
            this.TalkMIDI(0xB0, 0, 0x79); //Bank select Melodic
                                     //These don't sound different from the main bank to me

            //Change to different instrument
            for (var instrument = 27; instrument < 87; instrument++) {

                Debug.WriteLine(" Instrument: "+ instrument);

                this.TalkMIDI(0xC0, instrument, 0); //Set instrument number. 0xC0 is a 1 data byte command

                //Play notes from F#-0 (30) to F#-5 (90):
                for (var note = 30; note < 40; note++) {
                    Debug.WriteLine("N:" + note);

                    //Note on channel 1 (0x90), some note value (note), middle velocity (0x45):
                    this.NoteOn(0, note, 60);
                    Thread.Sleep(50);

                    //Turn off the note with a given off/release velocity
                    this.NoteOff(0, note, 60);
                    Thread.Sleep(50);
                }

                Thread.Sleep(100); //Delay between instruments
            }
        }
        public void PlayBasicDemo() {

            //Demo Basic MIDI instruments, GM1
            //=================================================================
            Debug.WriteLine("Basic Instruments");
            this.TalkMIDI(0xB0, 0, 0x00); //Default bank GM1

            //Change to different instrument
            for (var instrument = 0; instrument < 127; instrument++) {

                Debug.WriteLine(" Instrument: " + instrument);

                this.TalkMIDI(0xC0, instrument, 0); //Set instrument number. 0xC0 is a 1 data byte command

                //Play notes from F#-0 (30) to F#-5 (90):
                for (var note = 30; note < 40; note++) {
                    Debug.WriteLine("N:" + note);

                    //Note on channel 1 (0x90), some note value (note), middle velocity (0x45):
                    this.NoteOn(0, note, 60);
                    Thread.Sleep(50);

                    //Turn off the note with a given off/release velocity
                    this.NoteOff(0, note, 60);
                    Thread.Sleep(50);
                }

                Thread.Sleep(100); //Delay between instruments
            }
        }
        //Send a MIDI note-on message.  Like pressing a piano key
        //channel ranges from 0-15
        public void NoteOn(int channel, int note, int attack_velocity)
            => this.TalkMIDI((0x90 | channel), note, attack_velocity);

        //Send a MIDI note-off message.  Like releasing a piano key
        public void NoteOff(int channel, int note, int release_velocity)
            => this.TalkMIDI((0x80 | channel), note, release_velocity);

        //Plays a MIDI note. Doesn't check to see that cmd is greater than 127, or that data values are less than 127
        public void TalkMIDI(int cmd, int data1, int data2) {
            //digitalWrite(ledPin, HIGH);
            this.b1[0] = (byte)cmd;
            this.uart.Write(this.b1);
            this.b1[0] = (byte)data1;
            this.uart.Write(this.b1);

            //Some commands only have one data byte. All cmds less than 0xBn have 2 data bytes 
            //(sort of: http://253.ccarh.org/handout/midiprotocol/)
            if ((cmd & 0xF0) <= 0xB0) {
                this.b1[0] = (byte)data2;
                this.uart.Write(this.b1);
            }
            //digitalWrite(ledPin, LOW);
        }
    }
}
