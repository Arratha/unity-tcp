using System;
using System.Collections.Generic;
using System.Linq;
using Network.Messages.Models;

namespace Network.Messages.Processors.Factory
{
    public class MessageProcessorFactory : IMessageProcessorFactory
    {
        private readonly Dictionary<Type, object> _processors  = new();

        public MessageProcessorFactory(Dictionary<Type, object> processors)
        {
            foreach (var processorEntry in processors)
            {
                var messageType = processorEntry.Key;
                var processor = processorEntry.Value;

                if (!typeof(IMessage).IsAssignableFrom(messageType))
                {
                    throw new ArgumentException($"Key '{messageType.Name}' must implement IMessage.");
                }

                var processorType = processor.GetType();
                var processorInterface = processorType.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageProcessor<>));

                if (processorInterface == null)
                {
                    throw new ArgumentException($"Processor '{processorType.Name}' must implement IMessageProcessor<T>.");
                }

                var processorMessageType = processorInterface.GetGenericArguments()[0];

                if (processorMessageType != messageType)
                {
                    throw new ArgumentException($"Processor '{processorType.Name}' does not match the message type '{messageType.Name}'.");
                }

                _processors.Add(messageType, processor);
            }
        }

        public IMessageProcessor<TMessage> GetProcessor<TMessage>() where TMessage : IMessage
        {
            if (_processors.TryGetValue(typeof(TMessage), out var processor))
            {
                return (IMessageProcessor<TMessage>)processor;
            }

            throw new InvalidOperationException("Processor not found for this message type.");
        }

        public IMessageProcessor GetProcessor(Type type)
        {
            if (!typeof(IMessage).IsAssignableFrom(type))
            {
                throw new ArgumentException($"{type} must implement IMessage.");
            }
            
            if (_processors.TryGetValue(type, out var processor))
            {
                return (IMessageProcessor)processor;
            }

            throw new InvalidOperationException("Processor not found for this message type.");
        }
    }
}