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
        private bool _isClosing = false;

        public event Action<string> OnMessage;
        public event Action<bool> OnConnectionStatusChanged;

        private bool _isConnected = false;
        public bool IsConnected
        {
            get => _isConnected;
            private set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    OnConnectionStatusChanged?.Invoke(_isConnected);
                }
            }
        }

        public async Task InitAsync()
        {
            AudioServerConfig config = new AudioServerConfig();
            Console.WriteLine($"WebSocket URL: {config.FullUrl}");

            _ws = new ClientWebSocket();
            _cts = new CancellationTokenSource();

            try
            {
                await _ws.ConnectAsync(new Uri(config.FullUrl), _cts.Token);
                Console.WriteLine("✅ 与语音识别服务WebSocket连接成功");
                IsConnected = true;

                string initJson = @"{
                    ""context"":{""productId"":""279607454""},
                    ""request"":{
                        ""audio"":{""audioType"":""wav"",""sampleRate"":16000,""channel"":1,""sampleBytes"":2},
                        ""asr"":{""enableRealTimeFeedback"":true,""lmId"":""default"",""enableNumberConvert"":false}
                    }
                }";
                var bytes = Encoding.UTF8.GetBytes(initJson);
                await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _cts.Token);
                Console.WriteLine("✅ 初始化JSON已发送");

                _ = Task.Run(() => ReceiveLoopAsync(), _cts.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ WebSocket连接失败: {ex.Message}");
            }
        }

        public async Task SendBuffer(byte[] buffer, int offset, int count)
        {
            if (_ws == null || _ws.State != WebSocketState.Open)
            {
                Console.WriteLine("⚠️ WebSocket未连接，无法发送音频数据");
                return;
            }
            try
            {
                await _ws.SendAsync(new ArraySegment<byte>(buffer, offset, count), WebSocketMessageType.Binary, true, _cts.Token);
                Console.WriteLine($"🎙️ 已发送音频数据: {count} 字节");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 发送音频数据异常: {ex.Message}");
            }
        }

        private async Task ReceiveLoopAsync()
        {
            var buffer = new byte[4096];
            while (_ws != null && _ws.State == WebSocketState.Open)
            {
                try
                {
                    var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine("⚠️ WebSocket被远端关闭");
                        IsConnected = false;
                        await CloseAsync(); // 主动关闭
                        break;
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine("📥 收到消息: " + msg);
                        OnMessage?.Invoke(msg);
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("ℹ️ WebSocket接收被取消");
                    break;
                }
                catch (WebSocketException wex)
                {
                    Console.WriteLine($"❌ WebSocket连接断开: {wex.Message} 状态: {_ws.State}");
                    IsConnected = false;
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ WebSocket接收异常: {ex.Message}");
                    break;
                }
            }
            Console.WriteLine("🔌 接收循环结束");
            IsConnected = false;
        }

        public async Task CloseAsync()
        {
            if (_ws == null || _isClosing)
                return;

            _isClosing = true;
            Console.WriteLine("🛑 正在关闭WebSocket连接...");

            try
            {
                _cts.Cancel();
                IsConnected = false;

                if (_ws.State == WebSocketState.Open || _ws.State == WebSocketState.CloseReceived)
                {
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ WebSocket关闭异常: " + ex.Message);
            }
            finally
            {
                _ws.Dispose();
                _ws = null;
                _cts.Dispose();
                _cts = null;
                _isClosing = false;
                Console.WriteLine("✅ WebSocket连接已关闭");
            }
        }
    }
}