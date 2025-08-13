using Services.ImageParser.Parsers;

namespace Services.ImageParser.Factory
{
    public enum ImageType
    {
        PNG,
        JPG
    }
    
    public interface IImageParserFactory
    {
        public IImageParser GetParser(ImageType type);
    }
}