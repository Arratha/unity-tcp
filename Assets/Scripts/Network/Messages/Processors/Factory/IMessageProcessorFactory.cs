using System;
using Network.Messages.Models;

namespace Network.Messages.Processors.Factory
{
    public interface IMessageProcessorFactory
    {
        public IMessageProcessor<TMessage> GetProcessor<TMessage>() where TMessage : IMessage;

        public IMessageProcessor GetProcessor(Type type);
    }
}