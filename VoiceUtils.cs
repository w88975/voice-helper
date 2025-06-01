using System;
using System.Collections.Generic;
using NAudio.Wave;

namespace VoiceHelper
{
    public class VoiceUtils
    {
        /// <summary>
        /// 获取所有可用的音频输入设备（如麦克风）
        /// </summary>
        /// <returns>设备信息列表</returns>
        public static List<AudioInputDevice> GetInputDevices()
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
