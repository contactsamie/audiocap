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
            using (WasapiCapture capture = new WasapiLoopbackCapture())
            {
                _capture = new WasapiLoopbackCapture();
                _capture.Initialize();
                _w = new WaveWriter(file , _capture.WaveFormat);
                _capture.DataAvailable += (s, capData) => _w.Write(capData.Data, capData.Offset, capData.ByteCount);
                _capture.Start();
            }
        }
        public void StopRecord()
        {
            if (_w == null || _capture == null) return;
            _capture.Stop();
            _w.Dispose();
            _w = null;
            _capture.Dispose();
            _capture = null;
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

        readonly WaveOut _outputter = new WaveOut()
            {
                DesiredLatency = 5000 //arbitrary but <1k is choppy and >1e5 errors
                ,
                NumberOfBuffers = 1 // 1,2,4 all work...
                ,
                DeviceNumber = 0
            };
        public TimeSpan Play(string text)
        {
            var reader = new NAudio.Wave.AudioFileReader(text);
            _outputter.Init(reader);
            _outputter.Play();
            return reader.TotalTime;
        }


        public void Stop()
        {
            _outputter.Stop();
          
        }
        public void CutL(int positionMiliseconds,double totalSize, string text)
        {
            text = text.Replace("wav", "mp3");
            _outputter.Dispose();
            CutAnMp3File(text,TimeSpan.FromMilliseconds(positionMiliseconds), TimeSpan.FromMilliseconds(totalSize));
        }

        public void CutR(int positionMiliseconds, string text)
        {
            text = text.Replace("wav", "mp3");
            _outputter.Dispose();
            CutAnMp3File(text, TimeSpan.FromMilliseconds(0),  TimeSpan.FromMilliseconds(positionMiliseconds) );
        }

        public void CutAnMp3File(string fileName, TimeSpan start, TimeSpan end)
        {
            var startTimeSpan = start;
            var endTimeSpan = end;
            using (IWaveSource source = CodecFactory.Instance.GetCodec(fileName))
            using (var mediaFoundationEncoder = MediaFoundationEncoder.CreateWMAEncoder(source.WaveFormat, fileName.Replace(".", "") + "_cut.mp3"))
            {
                AddTimeSpan(source, mediaFoundationEncoder, startTimeSpan, endTimeSpan);
            }
        }
        private static void AddTimeSpan(IWaveSource source, IWriteable mediaFoundationEncoder, TimeSpan startTimeSpan, TimeSpan endTimeSpan)
        {
            source.SetPosition(startTimeSpan);

            int read = 0;
            long bytesToEncode = source.GetBytes(endTimeSpan - startTimeSpan);

            var buffer = new byte[source.WaveFormat.BytesPerSecond];
            while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                int bytesToWrite = (int)Math.Min(read, bytesToEncode);
                mediaFoundationEncoder.Write(buffer, 0, bytesToWrite);
                bytesToEncode -= bytesToWrite;
            }
        }

    }
}
