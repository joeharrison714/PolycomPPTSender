using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Threading;

namespace Packet.PacketConsole
{
//	public class PTTSender
//	{
//		const string DestinationAddress = "224.0.1.116";
//		const int DestinationPort = 5001;

//		public void Send()
//		{
//            string outputFile = @"D:\Development\Packet\Packet.PacketConsole\output.wav";
//            using (SpeechSynthesizer speaker = new SpeechSynthesizer())
//            {
//                speaker.Rate = 1;
//                //speaker.Volume = 100;
//                speaker.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
//                //var vs = speaker.GetInstalledVoices();

//                speaker.SetOutputToWaveFile(outputFile,
//                    new SpeechAudioFormatInfo(EncodingFormat.ULaw, 8000, 8, 1, 20, 2, null));


//                PromptBuilder builder = new PromptBuilder();
//                builder.AppendText("I am Commander William Riker of the Starship Enterprise. I am Commander William Riker of the Starship Enterprise. I am Commander William Riker of the Starship Enterprise.");

//                // Speak the prompt.
//                speaker.Speak(builder);
//            }


//            outputFile = @"D:\Development\Packet\Packet.PacketConsole\g711-ulaw-5s.wav";
//            byte[] bytes = System.IO.File.ReadAllBytes(outputFile);

//            List<byte[]> fileChunks = new List<byte[]>();
//            List<byte> thisChunk = new List<byte>();
//            for (int i = 0; i < bytes.Length; i++)
//            {
//                thisChunk.Add(bytes[i]);
                
//                if (thisChunk.Count == 160)
//                {
//                    fileChunks.Add(thisChunk.ToArray());
//                    thisChunk = new List<byte>();
//                }
//            }

//            if (thisChunk.Count>0)
//            {
//                fileChunks.Add(thisChunk.ToArray());
//            }

            

//            byte[] alertHeader = GenerateHeaderBytes(OpCode.Alert);

//			for (int i = 0; i < 31; i++)
//			{
//                Console.WriteLine("Alert");
//                SendPacket(alertHeader);
//				Thread.Sleep(30);
//			}

//            byte[] transmitHeader = GenerateTransmitHeaderBytes(Codec.G711U);
//            byte[] lastChunk = null;
//            int chunkIndex=0;
//            foreach (var chunk in fileChunks)
//            {
//                List<byte> packet = new List<byte>();

//                packet.AddRange(transmitHeader);

//                //packet.Add(202);
//                //packet.Add(124);
//                //packet.Add(149);
//                //packet.Add(94);

//                byte[] indexBytes = BitConverter.GetBytes(chunkIndex);

//                packet.AddRange(indexBytes);


//                if (lastChunk != null) packet.AddRange(lastChunk);

//                packet.AddRange(chunk);

//                Console.WriteLine("Transmit");
//                SendPacket(packet.ToArray());

//                lastChunk = chunk;

//                Thread.Sleep(30);
//                chunkIndex++;
//            }

//            Thread.Sleep(20);
            

//            byte[] endHeader = GenerateHeaderBytes(OpCode.EndOfTransmit);
//			for (int i = 0; i < 12; i++)
//			{
//                Console.WriteLine("EndOfTransmit");
//                SendPacket(endHeader);
//				Thread.Sleep(30);
//			}
//		}

//		private void SendPacket(byte[] bytes)
//		{
////            Console.WriteLine(bytes);
//  //          return;
//			using (Socket mSendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
//			{
//				mSendSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
//											new MulticastOption(IPAddress.Parse(DestinationAddress)));
//				mSendSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 255);
//				mSendSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
//				mSendSocket.Bind(new IPEndPoint(IPAddress.Any, DestinationPort));
//				IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(DestinationAddress), DestinationPort);
//				mSendSocket.Connect(ipep);

//				mSendSocket.Send(bytes, bytes.Length, SocketFlags.None);
//			}
//		}

//        private byte[] GenerateTransmitHeaderBytes(Codec codec)
//        {
//            List<byte> alertPacket = new List<byte>();
//            alertPacket.AddRange(GenerateHeaderBytes(OpCode.Transmit));

//            switch (codec)
//            {
//                case Codec.G711U:
//                    alertPacket.Add(0);
//                    break;
//                case Codec.G722:
//                    alertPacket.Add(9);
//                    break;
//                case Codec.G726QI:
//                    alertPacket.Add(253);
//                    break;
//            }

//            // flags
//            alertPacket.Add(111);

//            return alertPacket.ToArray();
//        }


//        private byte[] GenerateHeaderBytes(OpCode opCode)
//		{
//			List<byte> alertPacket = new List<byte>();

//			// Op Code
//			//alertPacket.Add(15);
//			switch (opCode)
//			{
//				case OpCode.Alert:
//					alertPacket.Add(15);
//					break;
//				case OpCode.Transmit:
//					alertPacket.Add(16);
//					break;
//				case OpCode.EndOfTransmit:
//					alertPacket.Add(255);
//					break;
//			}

//			// Channel Number
//			alertPacket.Add(26);

//			//MAC
//			alertPacket.Add(242); //F2
//			alertPacket.Add(17); // 11
//			alertPacket.Add(21); //15
//			alertPacket.Add(17); //11

//            // caller id length
//			alertPacket.Add(13);

//			alertPacket.Add(77); //4d
//			alertPacket.Add(101); // 65
//			alertPacket.Add(108); // 6c
//			alertPacket.Add(111); // 6f
//			alertPacket.Add(100); // 64
//			alertPacket.Add(121); // 79
//			alertPacket.Add(32); // 20
//			alertPacket.Add(77); // 4d
//			alertPacket.Add(101); // 65
//			alertPacket.Add(115); // 73
//			alertPacket.Add(101); // 65
//			alertPacket.Add(114); // 72
//			alertPacket.Add(118); // 76


//            alertPacket.Add(9); // 09

//            return alertPacket.ToArray();
//		}
//	}

}