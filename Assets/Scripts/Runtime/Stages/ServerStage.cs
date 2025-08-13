using System;
using System.Net;
using Global;
using Network.Client;
using Network.Messages.Parsers;
using Network.Messages.Processors.Factory;
using Network.Server;
using Network.Status;
using Utils.DI;
using ILogger = Services.Logger.ILogger;

namespace Runtime.Stages
{
    [Serializable]
    public class ServerStage
    {
        private IServiceCollection _serviceCollection;
        private IServer _server;

        public void Initialize(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public void StartServer()
        {
            StopServer();

            var port = ConfigLoader.LoadOrCreateConfig().port;
            var localAddress = IPAddress.Parse(Settings.Network.LocalIp);
            var endpoint = new IPEndPoint(localAddress, port);

            var logger = _serviceCollection.Resolve<ILogger>();

            _server = new TcpServer(endpoint, logger);
            _server.OnMessageReceived += ProcessMessage;
            _server.OnClientConnected += ProcessClient;
        }

        public void StopServer()
        {
            if (_server != null)
            {
                _server.OnMessageReceived -= ProcessMessage;
                _server.OnClientConnected -= ProcessClient;
            }

            _server?.Dispose();
            _server = null;
        }

        private void ProcessMessage(ClientId id, byte[] data)
        {
            var messageParser = _serviceCollection.Resolve<IMessageParser>();
            var messageProcessor = _serviceCollection.Resolve<IMessageProcessorFactory>();

            var parsedMessage = messageParser.Deserialize(data);
            var processor = messageProcessor.GetProcessor(parsedMessage.GetType());
            processor.Process(id, parsedMessage);
        }

        private void ProcessClient(ClientId id)
        {
            var logger = _serviceCollection.Resolve<ILogger>();
            var messageParser = _serviceCollection.Resolve<IMessageParser>();

            _ = new ClientAliveChecker(id, _server, logger, messageParser);
        }
    }
}