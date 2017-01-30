using Packet.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Packet.Core
{
    public class PTTSender
    {
        const string address = "224.0.1.116";
        const int port = 5001;

        private Stopwatch _stopwatch = new Stopwatch();

        public void Send(int channelNumber, string callerId, Dictionary<uint, byte[]> audioData, TimestampType tt, Codec codec)
        {
            string mac = GetMacAddress();

            string hostSerialNumber = string.Format("{0}{1}-{2}{3}-{4}{5}-{6}{7}", mac[4], mac[5], mac[6], mac[7], mac[8], mac[9], mac[10], mac[11]);

            {
                PolycomPTTPacket alertPacket = new PolycomPTTPacket();
                alertPacket.OpCode = OpCode.Alert;
                alertPacket.ChannelNumber = channelNumber;
                alertPacket.CallerID = callerId;
                alertPacket.HostSerialNumber = hostSerialNumber;

                for (int i = 0; i < 31; i++)
                {
                    //Console.WriteLine("Alert");
                    byte[] packetData = alertPacket.ToPacket(tt);

                    if (i > 0) WaitFor(30);

                    SendPacket(packetData);
                }
            }

            byte[] previousAudioData = null;

            foreach (var audioDataItem in audioData)
            {
                PolycomPTTPacket audioPacket = new PolycomPTTPacket();
                audioPacket.OpCode = OpCode.Transmit;
                audioPacket.ChannelNumber = channelNumber;
                audioPacket.CallerID = callerId;
                audioPacket.HostSerialNumber = hostSerialNumber;

                audioPacket.Codec = codec;
                audioPacket.Flags = "FA";
                audioPacket.SampleCount = audioDataItem.Key;

                //audioPacket.AudioData = audioDataItem.Value;
                byte[] nextPreviousAudioData = (byte[])audioDataItem.Value.Clone();

                audioPacket.AudioData = new byte[audioDataItem.Value.Length - 1];
                for(int i = 0; i < audioDataItem.Value.Length - 1; i++)
                {
                    audioPacket.AudioData[i] = audioDataItem.Value[i];
                }


                audioPacket.PreviousAudioData = previousAudioData;

                //Console.WriteLine("Transmit");
                byte[] packetData = audioPacket.ToPacket(tt);

                WaitFor(20);

                SendPacket(packetData);
                previousAudioData = (byte[])nextPreviousAudioData.Clone();
            }

            WaitFor(50);

            {
                PolycomPTTPacket endPacket = new PolycomPTTPacket();
                endPacket.OpCode = OpCode.EndOfTransmit;
                endPacket.ChannelNumber = channelNumber;
                endPacket.CallerID = callerId;
                endPacket.HostSerialNumber = hostSerialNumber;

                for (int i = 0; i < 12; i++)
                {
                    //Console.WriteLine("EndOfTransmit");
                    byte[] packetData = endPacket.ToPacket(tt);

                    if (i > 0) WaitFor(30);

                    SendPacket(packetData);
                }

            }


        }

        private void WaitFor(int milliseconds)
        {
            while (_stopwatch.ElapsedMilliseconds < milliseconds)
            {
                Thread.Sleep(1);
            }
        }

        private void SendPacket(byte[] packet)
        {
            _stopwatch.Restart();
            //return;
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

        private string GetMacAddress()
        {
            const int MIN_MAC_ADDR_LENGTH = 12;
            string macAddress = string.Empty;
            long maxSpeed = -1;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                //log.Debug(
                //    "Found MAC Address: " + nic.GetPhysicalAddress() +
                //    " Type: " + nic.NetworkInterfaceType);

                string tempMac = nic.GetPhysicalAddress().ToString();
                if (nic.Speed > maxSpeed &&
                    !string.IsNullOrEmpty(tempMac) &&
                    tempMac.Length >= MIN_MAC_ADDR_LENGTH)
                {
                    //log.Debug("New Max Speed = " + nic.Speed + ", MAC: " + tempMac);
                    maxSpeed = nic.Speed;
                    macAddress = tempMac;
                }
            }

            return macAddress;
        }
    }
}
