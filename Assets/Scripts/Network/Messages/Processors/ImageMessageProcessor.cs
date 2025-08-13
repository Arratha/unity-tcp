using Network.Client;
using Network.Messages.Models;
using Services.ImageParser.Factory;
using Services.ThreadDispatchers;
using UnityEngine.UI;

namespace Network.Messages.Processors
{
    //TODO: Actual implementation
    public class ImageMessageProcessor : IMessageProcessor<ImageMessage>
    {
        private readonly IImageParserFactory _imageParserFactory;
        private readonly IMainThreadDispatcher _mainThreadDispatcher;
        
        private readonly RawImage _image;
        
        public ImageMessageProcessor(IImageParserFactory imageParserFactory, IMainThreadDispatcher mainThreadDispatcher, RawImage image)
        {
            _imageParserFactory = imageParserFactory;
            _mainThreadDispatcher = mainThreadDispatcher;
            _image = image;
        }

        public void Process(ClientId client, ImageMessage message)
        {
            _mainThreadDispatcher.Enqueue(() => ProcessImage(message));
        }

        private void ProcessImage(ImageMessage message)
        {
            var parser = _imageParserFactory.GetParser(message.type);
            var texture = parser.Decode(message.data);
            
            _image.texture = texture;
        }
    }
}