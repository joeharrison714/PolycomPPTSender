using NAudio.Wave;
using Packet.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Packet.PacketConsole
{
	class Program
	{
		static void Main(string[] args)
		{
            var knownWorkingAudio = GetWorkingAudio();

            Dictionary<uint, byte[]> newAudioBytes = new Dictionary<uint, byte[]>();

            uint newTimestamp = 1908944;

            foreach (var kwa in knownWorkingAudio)
            {
                newAudioBytes.Add(newTimestamp, kwa.Value);
                newTimestamp += 160;
            }

            PTTSender pttSender1 = new PTTSender();
            pttSender1.Send(26, "Joe", newAudioBytes, Model.TimestampType.Try2);

            return;

            Dictionary<uint, byte[]> audioBytes = new Dictionary<uint, byte[]>();

            uint timestamp = 1908944;
            Model.TimestampType timestampType = Model.TimestampType.Try2;

            //string filename = Path.Combine(AssemblyDirectory + @"\..\..\..\TestAudio", "VirusAlert.wav");
            //string filename = Path.Combine(AssemblyDirectory + @"\..\..\..\TestAudio", "clockchime.ulaw.wav");
            //string filename = Path.Combine(AssemblyDirectory + @"\..\..\..\TestAudio", "lightning_announce.wav");
            string filename = Path.Combine(AssemblyDirectory + @"\..\..\..\TestAudio", "g711-ulaw-5s.wav");
            
            using (WaveFileReader reader = new WaveFileReader(filename))
            {
                Console.WriteLine(reader.WaveFormat.SampleRate);

                Console.WriteLine("Encoding: {0}", reader.WaveFormat.Encoding);
                Console.WriteLine("SampleRate: {0}", reader.WaveFormat.SampleRate);
                Console.WriteLine("Channels: {0}", reader.WaveFormat.Channels);
                Console.WriteLine("AverageBytesPerSecond: {0}", reader.WaveFormat.AverageBytesPerSecond);

                var bytesToRead = reader.WaveFormat.AverageBytesPerSecond / 50;

                int numberOfChunks = (int)Math.Ceiling((reader.Length + 0.0) / (bytesToRead + 0.0));

                //var timestampList = GetTimestamps();
                //int timestampListIndex = 0;

                int bytesRead = 0;
                byte[] readChunk = new byte[bytesToRead];
                do
                {
                    bytesRead = reader.Read(readChunk, 0, readChunk.Length);

                    //timestamp = timestampList[timestampListIndex];
                    

                    //Process the bytes here
                    audioBytes.Add(timestamp,  (byte[])readChunk.Clone());
                    timestamp += (uint)bytesToRead;

                    //timestampListIndex++;
                    //if (timestampListIndex >= timestampList.Count) break;
                }
                while (bytesRead != 0);

            }

            PTTSender pttSender = new PTTSender();
            pttSender.Send(26, "Joe", audioBytes, timestampType);

            //var pcmFormat = new WaveFormat(8000, 16, 1);
            //var ulawFormat = WaveFormat.CreateMuLawFormat(8000, 1);

            //Dictionary<uint, byte[]> audioBytes = new Dictionary<uint, byte[]>();
            //uint index = 1;

            //using (WaveFormatConversionStream pcmStm = new WaveFormatConversionStream(pcmFormat, new WaveFileReader(@"D:\Development\Packet\Packet.PacketConsole\pcm1608m.wav")))
            //{
            //    using (WaveFormatConversionStream ulawStm = new WaveFormatConversionStream(ulawFormat, pcmStm))
            //    {
            //        byte[] buffer = new byte[160];
            //        int bytesRead = ulawStm.Read(buffer, 0, 160);

            //        while (bytesRead > 0)
            //        {
            //            byte[] sample = new byte[bytesRead];
            //            Array.Copy(buffer, sample, bytesRead);
            //            audioBytes.Add(index, sample);
            //            index += 1;

            //            bytesRead = ulawStm.Read(buffer, 0, 160);
            //        }
            //    }
            //}
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

        static Dictionary<uint,byte[]> GetWorkingAudio()
        {
            string filename = AssemblyDirectory + @"\..\..\..\Sample\testing123.bin";

            using (FileStream stream = File.OpenRead(filename))
            {
                var formatter = new BinaryFormatter();
                var v = (Dictionary<uint, byte[]>)formatter.Deserialize(stream);
                stream.Close();
                return v;
            }
        }

        static List<uint> GetTimestamps()
        {
            List<uint> i = new List<uint>() { 488689914,
488730874,
488771763,
488812734,
488853747,
488894682,
488935645,
488976624,
489017595,
489058527,
489099378,
489140313,
489181430,
489222324,
489263327,
489304154,
489345210,
489386164,
489427166,
489468086,
489509085,
489549879,
489590900,
489631994,
489672947,
489713886,
489754747,
489795835,
489836794,
489877750,
489918709,
489959670,
490000635,
490041596,
490082421,
490123452,
490164346,
490205434,
490246233,
490287292,
490328187,
490369146,
490410164,
490451190,
490492090,
490533116,
490574078,
490615031,
490655833,
490696922,
490737822,
490778750,
490819804,
490860761,
490901589,
490942654,
490983518,
491024625,
491065513,
491106476,
491147290,
491188474,
491229364,
491270363,
491311221,
491352308,
491393272,
491434039,
491475160,
491516025,
491556917,
491598067,
491639033,
491679864,
491720882,
491761918,
491802815,
491843828,
491884587,
491925595,
491966590,
492007641,
492048621,
492089567,
492130527,
492171451,
492212478,
492253363,
492294315,
492335284,
492376188,
492417261,
492458202,
492499071,
492540146,
492581013,
492621973,
492662993,
492703934,
492744795,
492785917,
492826679,
492867698,
492908779,
492949679,
492990555,
493031609,
493072503,
493113427,
493154559,
493195422,
493236379,
493277367,
493318268,
493359287,
493400180,
493441116,
493482011,
493523127,
493563921,
493604949,
493646069,
493687000,
493727998,
493768956,
493809914,
493850867,
493891738,
493932754,
493973684,
494014555,
494055662,
494096505,
494137561,
494178414,
494219384,
494260383,
494301428,
494342334,
494383347,
494424217,
494465271,
494506174,
494547164,
494588158,
494629087,
494670010,
494711004,
494751900,
494792882,
494833880,
494874783,
494915736,
494956758,
494997717,
495038685,
495079679,
495120532,
495161592,
495202492,
495243510,
495284346,
495325220,
495366325,
495407324,
495448282,
495489247,
495530143,
495571195,
495612086,
495653109,
495693970,
495735006,
495775993,
495816923,
495857885,
495898812,
495939764,
495980664,
496021599,
496062687,
496103547,
496144473,
496185565,
496226490,
496267515,
496308337,
496349365,
496390388,
496431356,
496472310,
496513140,
496554232,
496595126,
496636156,
496677111,
496717997,
496758905,
496799868,
496840819,
496881787,
496922802,
496963700,
497004785,
497045758,
497086714,
497127533,
497168634,
497209526};
            return i;
        }
	}
}
