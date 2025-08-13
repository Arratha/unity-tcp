using System;

namespace Network.Client
{
    public interface IClientWrapper : IDisposable
    {
        public event Action<byte[]> OnMessageReceived;
        public event Action<byte[]> OnMessageSent;
        public event Action OnClientClosed;

        public void EnqueueMessage(byte[] message);
    }
}