using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Global;
using Network.Client;
using ILogger = Services.Logger.ILogger;
using LogType = Services.Logger.LogType;

namespace Network.Server
{
    public class TcpServer : IServer
    {
        private TcpListener _listener;
        private ConcurrentDictionary<ClientId, IClientWrapper> _clients = new();

        private ILogger _logger;

        private bool _isDisposed;

        private CancellationTokenSource _cts = new();

        public event Action<ClientId> OnClientConnected;
        public event Action<ClientId> OnClientDisconnected;
        public event Action<ClientId, byte[]> OnMessageReceived;
        public event Action<ClientId, byte[]> OnMessageSent;

        public TcpServer(IPEndPoint endPoint, ILogger logger)
        {
            _logger = logger;

            try
            {
                _listener = new TcpListener(endPoint);
                _listener.Start();

                _logger.Log(LogType.Message,
                    $"Start listening at address: {endPoint.Address} and port {endPoint.Port}");

                Task.Run(() => RunServerAsync(_cts.Token));
            }
            catch (Exception ex)
            {
                _logger.Log(ex);
                Dispose();

                throw;
            }
        }

        public void SendMessage(ClientId id, byte[] data)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpServer));
            }

            if (!_clients.TryGetValue(id, out var client))
            {
                _logger.Log(LogType.Exception, $"Client with id {id} not found");
                return;
            }

            try
            {
                client.EnqueueMessage(data);
            }
            catch (Exception ex)
            {
                _logger.Log(LogType.Exception,
                    $"Exception was encountered while trying to enqueue a message to the client {id}\n{ex}");
            }
        }

        public void DisconnectClient(ClientId id)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpServer));
            }

            if (!_clients.TryGetValue(id, out var client))
            {
                _logger.Log(LogType.Exception, $"Client with id {id} not found");
                return;
            }
            
            client.Dispose();
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

            _listener?.Stop();
            _listener = null;

            foreach (var clientId in _clients.Values.ToList())
            {
                clientId.Dispose();
            }

            _clients.Clear();

            OnClientConnected = null;
            OnClientDisconnected = null;
            OnMessageReceived = null;
            OnMessageSent = null;

            GC.SuppressFinalize(this);

            _logger.Log(LogType.Message, "Stop listening");
        }

        private async Task RunServerAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await ListenForClientsAsync(token);

                    await Task.Delay(Settings.Network.ServerProcessIntervalMs);
                }
                catch (OperationCanceledException)
                {
                    _logger.Log(LogType.Warning, "Server shutdown requested");
                }
                catch (Exception ex)
                {
                    _logger.Log(ex);
                }
            }
        }

        private async Task ListenForClientsAsync(CancellationToken token)
        {
            while (_listener.Pending() && !token.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync();

                if (!client.Connected)
                {
                    _logger.Log(LogType.Warning, "Connection attempt failed");
                    return;
                }

                var id = new ClientId(client);
                var clientWrapper = new TcpClientWrapper(client, _logger);

                if (_clients.TryAdd(id, clientWrapper))
                {
                    client.SendTimeout = Settings.Network.SendTimeout;
                    client.ReceiveTimeout = Settings.Network.ReceiveTimeout;

                    clientWrapper.OnMessageReceived += message => OnMessageReceived?.Invoke(id, message);
                    clientWrapper.OnMessageSent += message => OnMessageSent?.Invoke(id, message);
                    clientWrapper.OnClientClosed += () => CloseClient(id);

                    OnClientConnected?.Invoke(id);
                    
                    _logger.Log(LogType.Message, "Client has connected to server");
                }
                else
                {
                    clientWrapper.Dispose();
                }
            }
        }

        private void CloseClient(ClientId clientId)
        {
            if (!_clients.TryRemove(clientId, out _))
            {
                _logger.Log(LogType.Exception, $"Client with id {clientId} does not exist. Connection cannot be closed");
                return;
            }

            OnClientDisconnected?.Invoke(clientId);

            _logger.Log(LogType.Message, $"Connection with client {clientId} has been closed");
        }
        
        ~TcpServer()
        {
            Dispose();
        }
    }
}