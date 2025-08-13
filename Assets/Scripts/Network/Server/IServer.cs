using System;
using System.Net;
using Network.Client;

namespace Network.Server
{
    public interface IServer : IDisposable
    {
        public event Action<ClientId> OnClientConnected;
        public event Action<ClientId> OnClientDisconnected;
        public event Action<ClientId, byte[]> OnMessageReceived;
        public event Action<ClientId, byte[]> OnMessageSent;

        public void SendMessage(ClientId id, byte[] data);

        public void DisconnectClient(ClientId id);
    }
}