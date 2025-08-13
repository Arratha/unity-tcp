using Network.Client;
using Network.Messages.Models;

namespace Network.Messages.Processors
{
    public interface IMessageProcessor
    {
        public void Process(ClientId client, IMessage message);
    }

    public interface IMessageProcessor<in TMessage> : IMessageProcessor  where TMessage : IMessage
    { 
        void Process(ClientId client, TMessage message);
        
        void IMessageProcessor.Process(ClientId client, IMessage message) 
            => Process(client, (TMessage)message);
    }
}