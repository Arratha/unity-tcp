using System.Collections.Generic;
using System.Linq;
using Services.ImageParser.Parsers;

namespace Services.ImageParser.Factory
{
    public class ImageParserFactory : IImageParserFactory
    {
        private Dictionary<ImageType, IImageParser> _parsers;
        
        public ImageParserFactory(Dictionary<ImageType, IImageParser> parsers)
        {
            _parsers = parsers.ToDictionary(
                x => x.Key,
                x => x.Value);
        }

        public IImageParser GetParser(ImageType type)
        {
            if (!_parsers.TryGetValue(type, out var parser))
            {
                throw new KeyNotFoundException($"No image parser found for type: {type}");
            }

            return parser;
        }
    }
}