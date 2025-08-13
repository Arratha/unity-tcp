using System;
using System.Threading;
using System.Threading.Tasks;
using Global;
using Network.Client;
using Network.Messages.Models;
using Network.Messages.Parsers;
using Network.Server;
using Services.Logger;

namespace Network.Status
{
    public class ClientAliveChecker
    {
        private IServer _server;
        
        private readonly ClientId _client;
        private readonly ILogger _logger;
        private readonly IMessageParser _messageParser;
        
        private DateTime _lastSent;
        private DateTime _lastReceived;

        private CancellationTokenSource _cts = new();

        public ClientAliveChecker(ClientId client, IServer server, ILogger logger, IMessageParser messageParser)
        {
            _client = client;
            _server = server;
            _logger = logger;
            _messageParser = messageParser;
            
            _lastSent = DateTime.UtcNow;
            _lastReceived = DateTime.UtcNow.AddMilliseconds(Settings.Network.Heartbeat.InitialDelayMs);

            _server.OnMessageSent += HandleSent;
            _server.OnMessageReceived += HandleReceived;
            _server.OnClientDisconnected += Stop;

            _ = RunHeartbeatAsync(_cts.Token);
        }

        private async Task RunHeartbeatAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                
                if ((now - _lastSent).TotalMilliseconds > Settings.Network.Heartbeat.SentIntervalMs)
                {
                    var pingMessage = new PingMessage
                    {
                        time = now
                    };

                    var data = _messageParser.Serialize(pingMessage);
                    _server.SendMessage(_client, data);
                }

                if ((now - _lastReceived).TotalMilliseconds > Settings.Network.Heartbeat.ReceiveIntervalMs)
                {
                    _logger.Log(LogType.Warning, "Receive interval is exceeded");

                    _server.DisconnectClient(_client);
                    return;
                }
                
                await Task.Delay(Settings.Network.ClientProcessIntervalMs, token);
            }
        }

        private void HandleSent(ClientId client, byte[] _)
        {
            if (!_client.Equals(client))
            {
                return;
            }
            
            _lastSent = DateTime.UtcNow;
        }
        
        private void HandleReceived(ClientId client, byte[] _)
        {
            if (!_client.Equals(client))
            {
                return;
            }
            
            _lastReceived = DateTime.UtcNow;
        }

        private void Stop(ClientId client)
        {
            if (!_client.Equals(client))
            {
                return;
            }

            _server.OnMessageSent -= HandleSent;
            _server.OnMessageReceived -= HandleReceived;
            _server.OnClientDisconnected -= Stop;
            _server = null;

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
}