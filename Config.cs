using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace VoiceHelper
{
    public class AudioServerConfig
    {
        public  string ServerSocketUrl { get; set; } = "ws://172.16.20.88:36003/runtime/v1/recognize"; // WebSocket服务地址
        public  string apiKey { get; set; } = "456"; // API密钥
        public  string deviceName { get; set; } = "15920096351";
        public  string lang { get; set; } = "cn";
        public  string productId { get; set; } = "123";
        public  string res { get; set; } = "aisichuan-mix";
        public  string clientId { get; set; } = "jvplz20nnr0da3838moxa9c1004zfbjw";

        public  string FullUrl
        {
            get
            {
                return $"{ServerSocketUrl}/?apiKey={apiKey}&deviceName={deviceName}&lang={lang}&productId={productId}&res={res}&clientId={clientId}";
            }
        }
    }

    public class Config
    {
        public static string WebSocketUrl { get; set; } = "ws://localhost:5000/ws/"; // WebSocket服务地址
        public static int RecordingSampleRate { get; set; } = 16000; // 录音采样率
        public static int RecordingChannels { get; set; } = 2; // 录音通道数
        public static int RecordingBufferSize { get; set; } = 1024; // 录音缓冲区大小
        // 更多配置项可以在这里添加
    }
}
