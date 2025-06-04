using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Wave;

namespace VoiceHelper
{
    /// <summary>
    /// 音频工具类，单例模式，支持实时录音
    /// </summary>
    public class VoiceUtils
    {
        private static readonly Lazy<VoiceUtils> _instance = new Lazy<VoiceUtils>(() => new VoiceUtils());
        public static VoiceUtils Instance => _instance.Value;

        private WaveInEvent _waveIn;
        private bool _isRecording = false;
        private int _channels = 1;

        /// <summary>
        /// 录音数据事件
        /// 参数: channels: List<ChannelBuffer>，每个元素包含channel索引和buffer
        /// </summary>
        public event Action<List<ChannelBuffer>> OnRecording;

        private VoiceUtils() { }

        /// <summary>
        /// 获取所有可用的音频输入设备（如麦克风）
        /// </summary>
        /// <returns>设备信息列表</returns>
        public List<AudioInputDevice> GetInputDevices()
        {
            var devices = new List<AudioInputDevice>();

            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                var caps = WaveIn.GetCapabilities(i);
                devices.Add(new AudioInputDevice
                {
                    DeviceIndex = i,
                    Name = caps.ProductName,
                    Channels = caps.Channels
                });
            }

            return devices;
        }

        /// <summary>
        /// 开始录音
        /// </summary>
        /// <param name="deviceIndex">设备索引</param>
        /// <param name="sampleRate">采样率，默认16000</param>
        /// <param name="channels">通道数，默认1</param>
        public void StartRecord(int deviceIndex = 0, int sampleRate = 16000, int channels = 1)
        {
            if (_isRecording)
                return;

            _channels = channels;

            _waveIn = new WaveInEvent
            {
                DeviceNumber = deviceIndex,
                WaveFormat = new WaveFormat(sampleRate, channels)
            };
            _waveIn.DataAvailable += WaveIn_DataAvailable;
            _waveIn.RecordingStopped += WaveIn_RecordingStopped;

            _waveIn.StartRecording();
            _isRecording = true;
            Console.WriteLine("录音已开始");
        }

        /// <summary>
        /// 停止录音
        /// </summary>
        public void StopRecord()
        {
            if (!_isRecording)
                return;

            _waveIn.StopRecording();
            // 事件在RecordingStopped中处理
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            // 实时打印buffer长度
            Console.WriteLine($"录音数据: {e.Buffer.Length} 字节, 有效数据: {e.BytesRecorded} 字节");

            List<ChannelBuffer> channelBuffers = new List<ChannelBuffer>();

            if (_channels == 2)
            {
                // 16位采样，每帧4字节（左2字节+右2字节）
                int bytesPerSample = 2; // 16bit
                int frameSize = bytesPerSample * 2; // 2通道
                int frames = e.BytesRecorded / frameSize;
                byte[] leftBuffer = new byte[frames * bytesPerSample];
                byte[] rightBuffer = new byte[frames * bytesPerSample];

                for (int i = 0; i < frames; i++)
                {
                    // 左声道
                    leftBuffer[i * bytesPerSample] = e.Buffer[i * frameSize];
                    leftBuffer[i * bytesPerSample + 1] = e.Buffer[i * frameSize + 1];
                    // 右声道
                    rightBuffer[i * bytesPerSample] = e.Buffer[i * frameSize + 2];
                    rightBuffer[i * bytesPerSample + 1] = e.Buffer[i * frameSize + 3];
                }

                channelBuffers.Add(new ChannelBuffer { Channel = 0, Buffer = leftBuffer });
                channelBuffers.Add(new ChannelBuffer { Channel = 1, Buffer = rightBuffer });
            }
            else if (_channels == 1)
            {
                // 单声道直接返回
                byte[] monoBuffer = new byte[e.BytesRecorded];
                Array.Copy(e.Buffer, 0, monoBuffer, 0, e.BytesRecorded);
                channelBuffers.Add(new ChannelBuffer { Channel = 0, Buffer = monoBuffer });
            }

            // 触发事件
            OnRecording?.Invoke(channelBuffers);
        }

        private void WaveIn_RecordingStopped(object sender, StoppedEventArgs e)
        {
            _waveIn.DataAvailable -= WaveIn_DataAvailable;
            _waveIn.RecordingStopped -= WaveIn_RecordingStopped;
            _waveIn.Dispose();
            _waveIn = null;

            Console.WriteLine("录音已停止");
            _isRecording = false;
        }
    }

    /// <summary>
    /// 单个声道的buffer数据
    /// </summary>
    public class ChannelBuffer
    {
        public int Channel { get; set; }
        public byte[] Buffer { get; set; }
    }

    /// <summary>
    /// 音频输入设备信息模型
    /// </summary>
    public class AudioInputDevice
    {
        public int DeviceIndex { get; set; }
        public string Name { get; set; }
        public int Channels { get; set; }

        public override string ToString()
        {
            return $"[{DeviceIndex}] {Name} ({Channels} channel(s))";
        }
    }
}
