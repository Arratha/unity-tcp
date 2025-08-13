using System;
using System.Threading;
using System.Threading.Tasks;
using Global;
using Network.Client;
using Network.Messages.Models;
using Network.Messages.Parsers;
using Services.Logger;

namespace Network.Status
{
    public class ServerAliveChecker
    {
        private IClientWrapper _client;
        
        private readonly ILogger _logger;
        private readonly IMessageParser _messageParser;
        
        private DateTime _lastSent;
        private DateTime _lastReceived;

        private CancellationTokenSource _cts = new();

        public ServerAliveChecker(IClientWrapper client, ILogger logger, IMessageParser messageParser)
        {
            _client = client;
            _logger = logger;
            _messageParser = messageParser;
            
            _lastSent = DateTime.UtcNow;
            _lastReceived = DateTime.UtcNow.AddMilliseconds(Settings.Network.Heartbeat.InitialDelayMs);

            _client.OnMessageSent += HandleSent;
            _client.OnMessageReceived += HandleReceived;
            _client.OnClientClosed += Stop;

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
                    _client.EnqueueMessage(data);
                }

                if ((now - _lastReceived).TotalMilliseconds > Settings.Network.Heartbeat.ReceiveIntervalMs)
                {
                    _logger.Log(LogType.Warning, "Receive interval is exceeded");

                    _client.Dispose();
                    return;
                }
                
                await Task.Delay(Settings.Network.ClientProcessIntervalMs, token);
            }
        }

        private void HandleSent(byte[] _)
        {
            _lastSent = DateTime.UtcNow;
        }
        
        private void HandleReceived(byte[] _)
        {
            _lastReceived = DateTime.UtcNow;
        }

        private void Stop()
        {
            _client.OnMessageSent -= HandleSent;
            _client.OnMessageReceived -= HandleReceived;
            _client.OnClientClosed -= Stop;
            _client = null;
            
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
}