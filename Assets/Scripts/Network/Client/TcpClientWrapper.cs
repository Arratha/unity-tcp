using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Extensions;
using Global;
using ILogger = Services.Logger.ILogger;
using LogType = Services.Logger.LogType;

namespace Network.Client
{
    public class TcpClientWrapper : IClientWrapper
    {
        private readonly System.Net.Sockets.TcpClient _client;
        private readonly ConcurrentQueue<byte[]> _pendingMessages = new();

        private readonly CancellationTokenSource _cts = new();
        private readonly ILogger _logger;

        private bool _isDisposed;

        public event Action<byte[]> OnMessageReceived;
        public event Action<byte[]> OnMessageSent;
        public event Action OnClientClosed;

        public TcpClientWrapper(System.Net.Sockets.TcpClient client, ILogger logger)
        {
            _client = client;
            _logger = logger;
            
            _ = ProcessClientAsync(_cts.Token);
        }

        public TcpClientWrapper(IPEndPoint endPoint, ILogger logger)
        {
            _logger = logger;

            try
            {
                _client = new System.Net.Sockets.TcpClient();
                _client.Connect(endPoint);

                _logger.Log(LogType.Message,
                    $"Client connected to address {endPoint.Address} and port {endPoint.Port}");

                _ = ProcessClientAsync(_cts.Token);
            }
            catch (Exception ex)
            {
                _logger.Log(ex);
                Dispose();

                throw;
            }
        }

        public void EnqueueMessage(byte[] message)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpClientWrapper));
            }

            if (message == null || message.Length == 0)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (!_client.Connected)
            {
                throw new InvalidOperationException("Client is not connected");
            }

            _pendingMessages.Enqueue(message);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            _cts.Cancel();
            _cts.Dispose();
            
            try
            {
                if (_client?.Connected == true)
                {
                    _client?.Client.Shutdown(SocketShutdown.Send); 
                    _client?.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogType.Exception, $"Error while closing client: {ex}");
            }
            finally
            {
                _client?.Dispose();
                _pendingMessages.Clear();

                OnClientClosed?.Invoke();
        
                OnMessageReceived = null;
                OnClientClosed = null;

                GC.SuppressFinalize(this);
            }
        }

        private async Task ProcessClientAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            { 
                try
                {
                    await Task.WhenAll(
                        SendMessagesAsync(token),
                        ListenForMessagesAsync(token));
                    
                    await Task.Delay(Settings.Network.ClientProcessIntervalMs);
                }
                catch (OperationCanceledException)
                {
                    _logger.Log(LogType.Warning, "Client shutdown requested");
                }
                catch (Exception ex)
                {
                    _logger.Log(ex);
                    Dispose();
                }
            }
        }
        
        private async Task SendMessagesAsync(CancellationToken token)
        {
            var sentCount = 0;

            while (!token.IsCancellationRequested)
            {
                if (!_client.Connected)
                {
                    throw new InvalidOperationException("Client is not connected");
                }
                
                if (sentCount >= Settings.Network.SendBatch || !_pendingMessages.TryDequeue(out var message))
                {
                    break;
                }

                try
                {
                    var stream = _client.GetStream();

                    var size = BitConverter.GetBytes(message.Length);

                    await stream.WriteAsync(size);
                    await stream.WriteAsync(message);
                    
                    OnMessageSent?.Invoke(message);
                    
                    _logger.Log(LogType.Message, $"{message.Length} bytes was sent to client");

                    sentCount++;
                }
                catch (OperationCanceledException)
                {
                    _logger.Log(LogType.Warning, $"Sending stopped for client");
                }
            }
        }

        private async Task ListenForMessagesAsync(CancellationToken token)
        {
            var receivedCount = 0;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (!_client.Connected)
                    {
                        break;
                    }

                    if (receivedCount >= Settings.Network.ReceiveBatch)
                    {
                        break;
                    }

                    var stream = _client.GetStream();

                    if (!stream.DataAvailable)
                    {
                        break;
                    }

                    var lengthBuffer = new byte[4];
                    await stream.ReadExactlyAsync(lengthBuffer, 0, lengthBuffer.Length, token);

                    var messageLength = BitConverter.ToInt32(lengthBuffer);

                    if (messageLength < 0 || messageLength > Settings.Network.MaxMessageSize)
                    {
                        throw new InvalidDataException($"Invalid message size: {messageLength}");
                    }

                    var messageBuffer = ArrayPool<byte>.Shared.Rent(messageLength);

                    try
                    {
                        await stream.ReadExactlyAsync(messageBuffer, 0, messageLength, token);

                        var messageCopy = new byte[messageLength];
                        Buffer.BlockCopy(messageBuffer, 0, messageCopy, 0, messageLength);
                        OnMessageReceived?.Invoke(messageCopy);
                        
                        _logger.Log(LogType.Message, $"{messageLength} bytes was received from client");

                        receivedCount++;
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(messageBuffer);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Log(LogType.Warning, $"Listening stopped for client");
            }
        }

        ~TcpClientWrapper()
        {
            Dispose();
        }
    }
}