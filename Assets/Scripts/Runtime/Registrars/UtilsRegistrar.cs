using System;
using System.Collections.Generic;
using Services.ImageParser.Factory;
using Services.ImageParser.Parsers;
using Services.ThreadDispatchers;
using UnityEngine;
using Utils.DI;
using ILogger = Services.Logger.ILogger;
using Logger = Services.Logger.Logger;

namespace Runtime.Registrars
{
    [Serializable]
    public class UtilsRegistrar
    {
        public void Register(IServiceCollection collection)
        {
            //ImageParserFactory
            collection.RegisterTransient<IImageParserFactory>(() =>
            {
                var parsers = new Dictionary<ImageType, IImageParser>()
                {
                    [ImageType.JPG] = new JpgParser(),
                    [ImageType.PNG] = new PngParser()
                };

                return new ImageParserFactory(parsers);
            });

            //Logger
            collection.RegisterTransient<ILogger>(() => new Logger());

            //ThreadDispatcher
            var threadDispatcherHolder = new GameObject(nameof(MainThreadDispatcher));
            var threadDispatcher = threadDispatcherHolder.AddComponent<MainThreadDispatcher>();
            collection.RegisterSingleton<IMainThreadDispatcher>(threadDispatcher);
        }
    }
}