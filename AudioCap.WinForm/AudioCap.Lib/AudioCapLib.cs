using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;
using CSCore.Codecs;
using CSCore.Codecs.WAV;
using CSCore.SoundIn;
using NAudio.Lame;
using NAudio.Wave;
using WasapiLoopbackCapture = CSCore.SoundIn.WasapiLoopbackCapture;

using MediaFoundationEncoder = CSCore.MediaFoundation.MediaFoundationEncoder;

namespace AudioCap.Lib
{
    public class AudioCapLib
    {
        private WasapiCapture _capture;
        private WaveWriter _w;
        public void StartRecord(string file)
        {
                _capture = new WasapiLoopbackCapture();
                _capture.Initialize();
                _w = new WaveWriter(file , _capture.WaveFormat);
                _capture.DataAvailable += (s, capData) => _w.Write(capData.Data, capData.Offset, capData.ByteCount);
                _capture.Start();
        }
        public void StopRecord()
        {
            if (_capture != null)
            {
             _capture.Stop();
              _capture.Dispose();
              _capture = null;
            }
            if (_w != null)
            {
                _w.Dispose();
                _w = null;
            }
         
          
        }
        public void WaveToMp3(string waveFileName, string mp3FileName, int bitRate = 128)
        {
            using (var reader = new NAudio.Wave.WaveFileReader(waveFileName))
            using (var writer = new LameMP3FileWriter(mp3FileName, reader.WaveFormat, bitRate))
                reader.CopyTo(writer);
        }
        public void Mp3ToWave(string mp3FileName, string waveFileName)
        {
            using (var reader = new NAudio.Wave.Mp3FileReader(mp3FileName))
            using (var writer = new NAudio.Wave.WaveFileWriter(waveFileName, reader.WaveFormat))
                reader.CopyTo(writer);
        }
        public byte[] ConvertWavToMp3(byte[] wavFile)
        {
            using (var retMs = new MemoryStream())
            using (var ms = new MemoryStream(wavFile))
            using (var rdr = new NAudio.Wave.WaveFileReader(ms))
            using (var wtr = new LameMP3FileWriter(retMs, rdr.WaveFormat, 128))
            {
                rdr.CopyTo(wtr);
                return retMs.ToArray();
            }
        }
        public static void Mp4ToMp3(string waveFileName, string mp3FileName, int bitRate = 128)
        {
            using (var reader = new MediaFoundationReader(waveFileName))
            using (var writer = new LameMP3FileWriter(mp3FileName, reader.WaveFormat, bitRate))
                reader.CopyTo(writer);
        }

         WaveOut _outputter = new WaveOut()
            {
                DesiredLatency = 5000 //arbitrary but <1k is choppy and >1e5 errors
                ,
                NumberOfBuffers = 1 // 1,2,4 all work...
                ,
                DeviceNumber = 0
            };
        public TimeSpan Play(string text)
        {
             Reader = new NAudio.Wave.AudioFileReader(text);
            _outputter.Init(Reader);
            _outputter.Play();
            return Reader.TotalTime;
        }

        public AudioFileReader Reader { get; set; }


        public void Stop()
        {
            Reader.Dispose();
            Reader = null;
           
            _outputter.Stop();
            _outputter.Dispose();
            _outputter = null;
           _outputter =  new WaveOut()
            {
                DesiredLatency = 5000 //arbitrary but <1k is choppy and >1e5 errors
                ,
                NumberOfBuffers = 1 // 1,2,4 all work...
                ,
                DeviceNumber = 0
            };

        }
        public void TrimL(int positionMiliseconds,double totalSize, string text)
        {
            text = text.Replace("wav", "mp3");
           
            CutAnMp3File(text,TimeSpan.FromMilliseconds(positionMiliseconds), TimeSpan.FromMilliseconds(totalSize));
        }

        public void Trim(int from, int to, string text, string outputFileName)
        {
            text = text.Replace("wav", "mp3");
            _outputter.Dispose();
            CutAnMp3File(text, TimeSpan.FromMilliseconds(from),  TimeSpan.FromMilliseconds(to), outputFileName);
        }

        void TrimMp3(string inputPath, string outputPath, TimeSpan? begin, TimeSpan? end)
        {
            if (begin.HasValue && end.HasValue && begin > end)
                throw new ArgumentOutOfRangeException("end", "end should be greater than begin");

            using (var reader = new Mp3FileReader(inputPath))
            using (var writer = File.Create(outputPath))
            {
                Mp3Frame frame;
                while ((frame = reader.ReadNextFrame()) != null)
                    if (reader.CurrentTime >= begin || !begin.HasValue)
                    {
                        if (reader.CurrentTime <= end || !end.HasValue)
                            writer.Write(frame.RawData, 0, frame.RawData.Length);
                        else break;
                    }
            }
        }

       
        public void CutAnMp3File(string fileName, TimeSpan start, TimeSpan end,string outputFileName=null)
        {
          
            TrimMp3(fileName, outputFileName, start, end);
        }
       
    }
}
