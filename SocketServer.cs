using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VoiceHelper
{
    /// <summary>
    /// WebSocket服务，应用全局唯一，支持消息收发
    /// </summary>
    internal class SocketServer
    {
        private static readonly Lazy<SocketServer> _instance = new Lazy<SocketServer>(() => new SocketServer());
        public static SocketServer Instance => _instance.Value;

        private readonly HttpListener _httpListener;
        private readonly ConcurrentDictionary<string, WebSocket> _clients = new ConcurrentDictionary<string, WebSocket>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private const string ListenerPrefix = "http://localhost:5000/ws/";

        private SocketServer()
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(ListenerPrefix);
            Start();
        }

        /// <summary>
        /// 启动WebSocket服务
        /// </summary>
        private void Start()
        {
            Task.Run(async () =>
            {
                try
                {
                    _httpListener.Start();
                    Console.WriteLine("启动成功");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("HttpListener 启动失败: " + ex);
                    return;
                }
                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var context = await _httpListener.GetContextAsync();
                        if (context.Request.IsWebSocketRequest)
                        {
                            _ = HandleClientAsync(context);
                        }
                        else
                        {
                            context.Response.StatusCode = 400;
                            context.Response.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("监听异常: " + ex);
                    }
                }
            }, _cts.Token);
        }

        /// <summary>
        /// 处理客户端连接
        /// </summary>
        private async Task HandleClientAsync(HttpListenerContext context)
        {
            var wsContext = await context.AcceptWebSocketAsync(null);
            var ws = wsContext.WebSocket;
            var clientId = Guid.NewGuid().ToString();
            _clients[clientId] = ws;

            try
            {
                var buffer = new byte[4096];
                while (ws.State == WebSocketState.Open)
                {
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
                        break;
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        OnMessageReceived?.Invoke(clientId, msg);
                    }
                }
            }
            catch (Exception)
            {
                // 忽略异常
            }
            finally
            {
                WebSocket removed;
                _clients.TryRemove(clientId, out removed);
                if (ws != null)
                    ws.Dispose();
            }
        }

        /// <summary>
        /// 向所有客户端发送消息
        /// </summary>
        public async Task BroadcastAsync(string message)
        {
            var data = Encoding.UTF8.GetBytes(message);
            var tasks = new ConcurrentBag<Task>();
            foreach (var ws in _clients.Values)
            {
                if (ws.State == WebSocketState.Open)
                {
                    tasks.Add(ws.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, CancellationToken.None));
                }
            }
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 向指定客户端发送消息
        /// </summary>
        public async Task SendToClientAsync(string clientId, string message)
        {
            WebSocket ws;
            if (_clients.TryGetValue(clientId, out ws) && ws.State == WebSocketState.Open)
            {
                var data = Encoding.UTF8.GetBytes(message);
                await ws.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        /// <summary>
        /// 消息接收事件
        /// </summary>
        public event Action<string, string> OnMessageReceived;

        /// <summary>
        /// 停止服务
        /// </summary>
        public void Stop()
        {
            _cts.Cancel();
            _httpListener.Stop();
            foreach (var ws in _clients.Values)
            {
                if (ws.State == WebSocketState.Open)
                {
                    ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server stopped", CancellationToken.None).Wait();
                }
            }
            _clients.Clear();
        }
    }
}
