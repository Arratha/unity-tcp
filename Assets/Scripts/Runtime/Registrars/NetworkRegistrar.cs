using System;
using System.Collections.Generic;
using Network.Messages.Models;
using Network.Messages.Parsers;
using Network.Messages.Processors;
using Network.Messages.Processors.Factory;
using Services.ImageParser.Factory;
using Services.ThreadDispatchers;
using UnityEngine;
using UnityEngine.UI;
using Utils.DI;

namespace Runtime.Registrars
{
    [Serializable]
    public class NetworkRegistrar
    {
        //Temp
        [SerializeField] private RawImage image;
        
        public void Register(IServiceCollection collection)
        {
            //MessageParser
            collection.RegisterTransient<IMessageParser>(() => new MessageParser());

            //MessageProcessor
            collection.RegisterTransient<IMessageProcessorFactory>(() =>
            {
                var imageParserFactory = collection.Resolve<IImageParserFactory>();
                var threadDispatcher = collection.Resolve<IMainThreadDispatcher>();
                
                var processors = new Dictionary<Type, object>()
                {
                    [typeof(PingMessage)] = new PingMessageProcessor(),
                    [typeof(ImageMessage)] = new ImageMessageProcessor(imageParserFactory, threadDispatcher, image)
                };

                return new MessageProcessorFactory(processors);
            });
        }
    }
}