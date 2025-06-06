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
        private bool _autoReconnect = true;
        private const int MaxReconnectAttempts = 5;
        private const int ReconnectDelayMs = 3000;

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

        public async Task InitAsync(bool autoReconnect = true)
        {
            _autoReconnect = autoReconnect;
            await ConnectWithRetryAsync();
        }

        private async Task ConnectWithRetryAsync(int attemptCount = 0)
        {
            if (_isClosing) return;

            try
            {
                if (_ws != null)
                {
                    _ws.Dispose();
                    _ws = null;
                }

                if (_cts != null)
                {
                    _cts.Dispose();
                }
                _cts = new CancellationTokenSource();

                AudioServerConfig config = new AudioServerConfig();
                Console.WriteLine($"WebSocket URL: {config.FullUrl}");

                _ws = new ClientWebSocket();
                await _ws.ConnectAsync(new Uri(config.FullUrl), _cts.Token);
                Console.WriteLine($"✅ 与语音识别服务WebSocket连接成功 (尝试次数: {attemptCount + 1})");
                IsConnected = true;

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
                Console.WriteLine("✅ 初始化JSON已发送");

                _ = Task.Run(() => ReceiveLoopAsync(), _cts.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ WebSocket连接失败: {ex.Message}");
                IsConnected = false;

                if (_autoReconnect && attemptCount < MaxReconnectAttempts)
                {
                    Console.WriteLine($"⏳ {ReconnectDelayMs / 1000}秒后尝试重连 ({attemptCount + 1}/{MaxReconnectAttempts})");
                    await Task.Delay(ReconnectDelayMs);
                    await ConnectWithRetryAsync(attemptCount + 1);
                }
                else if (attemptCount >= MaxReconnectAttempts)
                {
                    Console.WriteLine("❌ 重连次数已达上限，停止重连");
                }
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
                await HandleConnectionFailure();
            }
        }

        private async Task HandleConnectionFailure()
        {
            IsConnected = false;
            if (_autoReconnect && !_isClosing)
            {
                Console.WriteLine("🔄 检测到连接断开，准备重连");
                await ConnectWithRetryAsync();
            }
        }

        private async Task ReceiveLoopAsync()
        {
            var buffer = new byte[4096];
            while (_ws != null && _ws.State == WebSocketState.Open && !_isClosing)
            {
                try
                {
                    var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine("⚠️ WebSocket被远端关闭 v2t");
                        await HandleConnectionFailure();
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
                    Console.WriteLine($"❌ WebSocket连接断开: {wex.Message} 状态: {_ws?.State}");
                    await HandleConnectionFailure();
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ WebSocket接收异常: {ex.Message}");
                    await HandleConnectionFailure();
                    break;
                }
            }
            Console.WriteLine("🔌 接收循环结束");
            if (!_isClosing)
            {
                await HandleConnectionFailure();
            }
        }

        public async Task CloseAsync()
        {
            if (_ws == null || _isClosing)
                return;

            _isClosing = true;
            _autoReconnect = false; // 禁用重连
            Console.WriteLine("🛑 正在关闭WebSocket连接...");

            try
            {
                _cts?.Cancel();
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
                if (_ws != null)
                {
                    _ws.Dispose();
                    _ws = null;
                }
                if (_cts != null)
                {
                    _cts.Dispose();
                    _cts = null;
                }
                _isClosing = false;
                Console.WriteLine("✅ WebSocket连接已关闭");
            }
        }
    }
}