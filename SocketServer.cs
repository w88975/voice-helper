using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VoiceHelper
{
    public class ClientInfo
    {
        public string Id { get; set; }
        public WebSocket Socket { get; set; }
        public DateTime ConnectedAt { get; set; }
        public string RemoteEndPoint { get; set; }
    }

    /// <summary>
    /// WebSocket服务，应用全局唯一，支持消息收发
    /// </summary>
    internal class SocketServer
    {
        private static readonly Lazy<SocketServer> _instance = new Lazy<SocketServer>(() => new SocketServer());
        public static SocketServer Instance => _instance.Value;

        private readonly HttpListener _httpListener;
        private readonly ConcurrentDictionary<string, ClientInfo> _clients = new ConcurrentDictionary<string, ClientInfo>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private const string ListenerPrefix = "http://localhost:5000/ws/";

        /// <summary>
        /// 在线客户端数量
        /// </summary>
        public int OnlineClients => _clients.Count;

        private bool _status = false;
        public bool Status
        {
            get => _status;
            private set
            {
                if (_status != value)
                {
                    _status = value;
                    OnStatusChanged?.Invoke(_status);
                }
            }
        }

        /// <summary>
        /// WebSocket服务状态变更事件
        /// </summary>
        public event Action<bool> OnStatusChanged;

        /// <summary>
        /// 客户端列表变更事件（如有客户端上线/下线时触发）
        /// </summary>
        public event Action<int> OnClientUpdated;

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
                    Status = true;
                    Console.WriteLine("启动成功");
                }
                catch (Exception ex)
                {
                    Status = false;
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
                    catch (HttpListenerException ex) when (_cts.IsCancellationRequested)
                    {
                        // Stop() 正常触发，忽略异常
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        // HttpListener 已被释放
                        break;
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
            var remoteEndPoint = context.Request.RemoteEndPoint?.ToString() ?? "";

            var clientInfo = new ClientInfo
            {
                Id = clientId,
                Socket = ws,
                ConnectedAt = DateTime.Now,
                RemoteEndPoint = remoteEndPoint
            };

            _clients[clientId] = clientInfo;
            OnClientUpdated?.Invoke(OnlineClients);

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
                ClientInfo removed;
                _clients.TryRemove(clientId, out removed);
                if (ws != null)
                    ws.Dispose();
                OnClientUpdated?.Invoke(OnlineClients);
            }
        }

        /// <summary>
        /// 向所有客户端发送消息
        /// </summary>
        public async Task BroadcastAsync(string message)
        {
            var data = Encoding.UTF8.GetBytes(message);
            var tasks = new ConcurrentBag<Task>();
            foreach (var client in _clients.Values)
            {
                var ws = client.Socket;
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
            ClientInfo client;
            if (_clients.TryGetValue(clientId, out client) && client.Socket.State == WebSocketState.Open)
            {
                var data = Encoding.UTF8.GetBytes(message);
                await client.Socket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        /// <summary>
        /// 消息接收事件
        /// </summary>
        public event Action<string, string> OnMessageReceived;

        /// <summary>
        /// 停止服务
        /// </summary>
        private bool _isStopped = false;

        public void Stop()
        {
            if (_isStopped) return; // 防止多次 Stop

            _isStopped = true;

            try
            {
                _cts.Cancel();
            }
            catch { }

            try
            {
                if (_httpListener.IsListening)
                {
                    _httpListener.Stop();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("HttpListener.Stop 异常: " + ex.Message);
            }

            Status = false;

            foreach (var client in _clients.Values)
            {
                try
                {
                    if (client.Socket.State == WebSocketState.Open)
                    {
                        client.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server stopped", CancellationToken.None).Wait();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("WebSocket 关闭异常: " + ex.Message);
                }
            }

            _clients.Clear();
            OnClientUpdated?.Invoke(OnlineClients);
        }
    }
}