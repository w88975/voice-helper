using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VoiceHelper
{
    internal class VoiceToText
    {
        private ClientWebSocket _ws;
        private CancellationTokenSource _cts;

        /// <summary>
        /// 语音识别服务返回消息事件，参数为文本内容
        /// </summary>
        public event Action<string> OnMessage;

        public async Task InitAsync()
        {
            AudioServerConfig config = new AudioServerConfig();
            Console.WriteLine($"WebSocket URL: {config.FullUrl}");

            _ws = new ClientWebSocket();
            _cts = new CancellationTokenSource();

            try
            {
                await _ws.ConnectAsync(new Uri(config.FullUrl), _cts.Token);
                Console.WriteLine("与语音识别服务WebSocket连接成功");

                // 连接成功后发送初始化json
                string initJson = @"{
                    ""context"":{""productId"":""279607454""},
                    ""request"":{
                        ""audio"":{""audioType"":""wav"",""sampleRate"":16000,""channel"":1,""sampleBytes"":2},
                        ""asr"":{""enableRealTimeFeedback"":true,""lmId"":""default"",""enableNumberConvert"":false}
                    }
                }";
                var bytes = Encoding.UTF8.GetBytes(initJson);
                await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _cts.Token);
                Console.WriteLine("初始化JSON已发送");

                // 启动接收消息的循环
                _ = Task.Run(() => ReceiveLoopAsync(), _cts.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine("WebSocket连接失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 发送音频buffer到WebSocket服务（wav裸流）
        /// </summary>
        /// <param name="buffer">音频数据</param>
        /// <param name="offset">起始位置</param>
        /// <param name="count">长度</param>
        public async Task SendBuffer(byte[] buffer, int offset, int count)
        {
            if (_ws == null || _ws.State != WebSocketState.Open)
            {
                Console.WriteLine("WebSocket未连接，无法发送音频数据");
                return;
            }
            try
            {
                await _ws.SendAsync(new ArraySegment<byte>(buffer, offset, count), WebSocketMessageType.Binary, true, _cts.Token);
                Console.WriteLine($"已发送音频数据: {count} 字节");

                // 发送一个空的Binary帧，表示音频数据发送完毕
                //await _ws.SendAsync(new ArraySegment<byte>(new byte[0]), WebSocketMessageType.Binary, true, _cts.Token);
                //Console.WriteLine("已发送音频结束标志（空Binary帧）");
            }
            catch (Exception ex)
            {
                Console.WriteLine("发送音频数据异常: " + ex.Message);
            }
        }

        private async Task ReceiveLoopAsync()
        {
            var buffer = new byte[4096];
            while (_ws.State == WebSocketState.Open)
            {
                try
                {
                    var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
                        break;
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine("收到消息: " + msg);
                        await Task.Delay(100);
                        OnMessage?.Invoke(msg);
                        // TODO: 处理语音识别返回内容
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("WebSocket接收异常: " + ex.Message);
                    break;
                }
            }
        }

        public async Task CloseAsync()
        {
            if (_ws != null && _ws.State == WebSocketState.Open)
            {
                _cts.Cancel();
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
                _ws.Dispose();
            }
        }
    }
}