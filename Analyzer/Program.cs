using Packet.Core;
using Packet.Model;
using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Analyzer
{
    class Program
    {
        static int packetIndex = 0;
        static StreamWriter sw;

        static PolycomPTTPacket alertPacket;
        static List<PolycomPTTPacket> audioPackets;
        static PolycomPTTPacket endPacket;

        static Dictionary<uint, byte[]> allAudioData;

        static void Main(string[] args)
        {
            sw = new StreamWriter(AssemblyDirectory + @"\..\..\..\Sample\parsed" + DateTime.Now.ToFileTime() + ".csv", false);
            allAudioData = new Dictionary<uint, byte[]>();
            audioPackets = new List<PolycomPTTPacket>();

            sw.Write("Time");
            sw.Write(",");
            sw.Write("Length");
            sw.Write(",");
            sw.Write("OpCode");
            sw.Write(",");
            sw.Write("ChannelNumber");
            sw.Write(",");
            sw.Write("HostSerialNumber");
            sw.Write(",");
            sw.Write("CallerID");
            sw.Write(",");
            sw.Write("Flags");
            sw.Write(",");
            sw.Write("SampleCountString");
            sw.Write(",");
            sw.Write("SampleCount");
            sw.Write(",");
            sw.Write("AudioDataLength");
            sw.Write(",");
            sw.Write("AudioData");
            sw.WriteLine();

            CaptureFileReaderDevice device = new CaptureFileReaderDevice(AssemblyDirectory + @"\..\..\..\Sample\sample2.pcap");

            // Register our handler function to the 'packet arrival' event
            device.OnPacketArrival += Device_OnPacketArrival;

            Console.WriteLine();

            // Start capture 'INFINTE' number of packets
            // This method will return when EOF reached.
            device.Capture();

            sw.Flush();
            sw.Close();
            sw.Dispose();

            string filename = AssemblyDirectory + @"\..\..\..\Sample\testing123_Fixed.bin";
            using (FileStream stream = File.Create(filename))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, allAudioData);
                stream.Close();
            }

            using (FileStream stream = File.OpenRead(filename))
            {
                var formatter = new BinaryFormatter();
                var v = (Dictionary<uint,byte[]>)formatter.Deserialize(stream);
                stream.Close();
            }

            //Console.WriteLine(allAudioData);

            //PTTSender sender = new PTTSender();
            //sender.Send(26, "Joe", allAudioData);

            //File.WriteAllBytes(@"D:\Development\Packet\Sample\audio.wav", allBytes.ToArray());

            //for (int i = 0; i < 31; i++)
            //{
            //    alertPacket.CallerID = "Joe";
            //    Console.WriteLine("Alert");
            //    SendPacket(alertPacket.ToPacket());
            //    Thread.Sleep(30);
            //}

            //foreach(var audioPacket in audioPackets)
            //{
            //    audioPacket.CallerID = "Joe";
            //    Console.WriteLine("Transmit");
            //    SendPacket(audioPacket.ToPacket());
            //    Thread.Sleep(30);
            //}

            //Thread.Sleep(20);

            //for (int i = 0; i < 12; i++)
            //{
            //    endPacket.CallerID = "Joe";
            //    Console.WriteLine("EndOfTransmit");
            //    SendPacket(endPacket.ToPacket());
            //    Thread.Sleep(30);
            //}
        }

        private static void SendPacket(byte[] packet)
        {
            string address = "224.0.1.116";
            int port = 5001;

            using (Socket mSendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                mSendSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
                                            new MulticastOption(IPAddress.Parse(address)));
                mSendSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 255);
                mSendSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                mSendSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(address), port);
                mSendSocket.Connect(ipep);


                byte[] bytes = packet;
                mSendSocket.Send(bytes, bytes.Length, SocketFlags.None);
            }
        }

        private static void Device_OnPacketArrival(object sender, SharpPcap.CaptureEventArgs e)
        {
            var packetData = e.Packet.Data;

            sw.Write(e.Packet.Timeval.Date);
            sw.Write(",");
            sw.Write(packetData.Length);
            sw.Write(",");

            Console.WriteLine(packetIndex + "\t" + packetData.Length);

            PolycomPTTPacket packet = PolycomPTTPacket.Parse(packetData);

            sw.Write(packet.OpCode.ToString());
            sw.Write(",");
            sw.Write(packet.ChannelNumber);
            sw.Write(",");
            sw.Write(packet.HostSerialNumber);
            sw.Write(",");
            sw.Write(packet.CallerID);
            sw.Write(",");
            sw.Write(packet.Flags);
            sw.Write(",");
            sw.Write(packet.SampleCountString);
            sw.Write(",");
            sw.Write(packet.SampleCount);
            sw.Write(",");
            if (packet.AudioData != null) sw.Write(packet.AudioData.Length);
            sw.Write(",");
            sw.Write(packet.AudioDataString);
            sw.WriteLine();
            //Console.WriteLine(",");

            if (packet.AudioData != null)
            {
                allAudioData.Add(packet.SampleCount, packet.AudioData);
            }

            byte[] oldPacket = new byte[packetData.Length - 42];
            for(int i=0; i < (packetData.Length - 42); i++)
            {
                oldPacket[i] = packetData[i + 42];
            }

            

            byte[] newPacket = packet.ToPacket(TimestampType.Try1);

            //oldPacket = oldPacket.Take(newPacket.Length).ToArray();

            var isEqual = oldPacket.SequenceEqual(newPacket);

            if (packet.OpCode== OpCode.Alert)
            {
                alertPacket = packet;
            }
            else if (packet.OpCode == OpCode.Transmit)
            {
                audioPackets.Add(packet);
            }
            else if(packet.OpCode == OpCode.EndOfTransmit)
            {
                endPacket = packet;
            }

                packetIndex++;
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
