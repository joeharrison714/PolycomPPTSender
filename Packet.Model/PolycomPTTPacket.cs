using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packet.Model
{
    public class PolycomPTTPacket
    {
        public OpCode OpCode { get; set; }

        /// <summary>
        /// The channels range from 1 – 50, with channels 1 – 25 for PTT, and channels 26 – 50 for paging.The 
        /// PTT/Paging feature enables users to broadcast messages with a certain priority level: Normal, Priority, or
        /// Emergency.By default, the PTT feature treats channel 24 as a Priority channel and channel 25 as an
        /// Emergency channel while the Paging feature treats channel 49 as the Priority channel and channel 50 as
        /// the Emergency channel.The Priority and Emergency channels can be changed by administrators.
        /// </summary>
        public int ChannelNumber { get; set; }

        /// <summary>
        /// The host serial number field is 4 bytes and represents the last 4 bytes of the serial number/MAC address
        /// of the broadcasting phone.This field is used for contention resolution – when multiple phones begin
        /// broadcasting on the same channel at the same time, the phone with the lowest serial number continues
        /// to broadcast and all other phones will stop broadcasting.Any 32 bit number can be used in place of the
        /// serial number as long as its value is guaranteed to be unique among the multicast participants.
        /// </summary>
        public string HostSerialNumber { get; set; }

        public string CallerID { get; set; }

        public Codec Codec { get; set; }

        public string Flags { get; set; }

        public uint SampleCount { get; set; }
        public string SampleCountString { get; set; }

        public byte[] PreviousAudioData { get; set; }

        public byte[] AudioData { get; set; }

        public string AudioDataString { get; set; }

        public byte[] ToPacket(TimestampType tt)
        {
            return ToPacket(this, tt);
        }

        public static byte[] ToPacket(PolycomPTTPacket packet, TimestampType tt)
        {
            List<byte> alertPacket = new List<byte>();

            // Op Code
            //alertPacket.Add(15);
            switch (packet.OpCode)
            {
                case OpCode.Alert:
                    alertPacket.Add(15);
                    break;
                case OpCode.Transmit:
                    alertPacket.Add(16);
                    break;
                case OpCode.EndOfTransmit:
                    alertPacket.Add(255);
                    break;
            }

            // Channel Number
            alertPacket.Add((byte)packet.ChannelNumber);

            //MAC
            string hsn = packet.HostSerialNumber;
            string[] hex = hsn.Split('-');
            //int value = Convert.ToInt32(hex[0], 16);

            alertPacket.Add((byte)Convert.ToInt32(hex[0], 16)); //F2
            alertPacket.Add((byte)Convert.ToInt32(hex[1], 16)); // 11
            alertPacket.Add((byte)Convert.ToInt32(hex[2], 16)); //15
            alertPacket.Add((byte)Convert.ToInt32(hex[3], 16)); //11

            // caller id length
            alertPacket.Add(13);

            char[] caller = packet.CallerID.ToCharArray();
            for (int i = 0; i < 13; i++)
            {
                if (i < caller.Length)
                {
                    alertPacket.Add((byte)caller[i]);
                }
                else
                {
                    alertPacket.Add(0);
                }
            }

            if (packet.OpCode == OpCode.Transmit)
            {
                // null
                alertPacket.Add(9); // 09

                // codec
                if (packet.Codec == Codec.G711U)
                {
                    alertPacket.Add(0x00);
                }
                else if (packet.Codec == Codec.G722)
                {
                    alertPacket.Add(0x09);
                }
                else if (packet.Codec == Codec.G726QI)
                {
                    alertPacket.Add(0xFF);
                }

                // flags
                int flagsValue = Convert.ToInt32(packet.Flags, 16);
                alertPacket.Add((byte)flagsValue);

                // RTP info
                byte[] sampleBytes = BitConverter.GetBytes(packet.SampleCount);
                Array.Reverse(sampleBytes);

                if (tt == TimestampType.Try1)
                {
                    alertPacket.AddRange(sampleBytes);
                }
                else if (tt == TimestampType.Try2)
                {
                    alertPacket.Add(sampleBytes[1]);
                    alertPacket.Add(sampleBytes[2]);
                    alertPacket.Add(sampleBytes[3]);
                    alertPacket.Add(0);
                }

                //redundant audio
                if (packet.PreviousAudioData != null)
                {
                    alertPacket.AddRange(packet.PreviousAudioData);
                }

                //audio
                alertPacket.AddRange(packet.AudioData);
            }

            return alertPacket.ToArray();
        
        }

        public static PolycomPTTPacket Parse(byte[] data)
        {
            PolycomPTTPacket packet = new PolycomPTTPacket();

            int offset = 42;

            byte opCodeByte = data[offset + 0];
            if (opCodeByte == 0x0F)
            {
                packet.OpCode = OpCode.Alert;
            }
            else if (opCodeByte == 0x10)
            {
                packet.OpCode = OpCode.Transmit;
            }
            else if (opCodeByte == 0xFF)
            {
                packet.OpCode = OpCode.EndOfTransmit;
            }

            byte channel = data[offset + 1];
            packet.ChannelNumber = channel;

            byte[] serial = new byte[4];
            serial[0] = data[offset + 2];
            serial[1] = data[offset + 3];
            serial[2] = data[offset + 4];
            serial[3] = data[offset + 5];
            packet.HostSerialNumber = ByteArrayToString(serial);

            int callerIdLength = data[offset + 6];
            int currentOffset = 0;
            string callerId = "";
            for (int i = 0; i < callerIdLength; i++)
            {
                currentOffset = offset + 7 + i;
                byte cid = data[currentOffset];
                
                if (cid == 0) continue;
                callerId += System.Text.Encoding.ASCII.GetString(new[] { cid });
            }
            packet.CallerID = callerId;

            if (packet.OpCode == OpCode.Transmit)
            {
                currentOffset += 1;

                byte nullChar = data[currentOffset];

                if (nullChar != 9)
                {
                    Console.WriteLine("wtf!");
                }
                currentOffset += 1;

                byte codec = data[currentOffset];
                if (codec == 0x00)
                {
                    packet.Codec = Codec.G711U;
                }
                else if (codec == 0x09)
                {
                    packet.Codec = Codec.G722;
                }
                else if (codec == 0xFF)
                {
                    packet.Codec = Codec.G726QI;
                }
                currentOffset += 1;


                byte flags = data[currentOffset];
                currentOffset += 1;
                packet.Flags = ByteArrayToString(new byte[] { flags });

                byte[] sampleCount = new byte[4];
                sampleCount[0] = data[currentOffset];
                currentOffset += 1;
                sampleCount[1] = data[currentOffset];
                currentOffset += 1;
                sampleCount[2] = data[currentOffset];
                currentOffset += 1;
                sampleCount[3] = data[currentOffset];
                currentOffset += 1;



                packet.SampleCountString = ByteArrayToString(sampleCount);
                uint left = Convert.ToUInt32(packet.SampleCountString.Replace("-",""), 16);
                //int right = BitConverter.ToUInt16(sampleCountRight, 0);
                //float f = float.Parse(left + "." + right);
                packet.SampleCount = left;



                List<byte> bytes = new List<byte>();
                for(int i = currentOffset; i < data.Length; i++)
                {
                    bytes.Add(data[i]);
                }
                packet.AudioData = bytes.ToArray();

                packet.AudioDataString = ByteArrayToString(packet.AudioData);
            }

            return packet;
        }

        private static string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex;
        }
    }

    public enum OpCode
    {
        Alert,
        Transmit,
        EndOfTransmit
    }

    public enum Codec
    {
        G711U,
        G722,
        G726QI
    }

    public enum TimestampType
    {
        Try1,
        Try2
    }
}
