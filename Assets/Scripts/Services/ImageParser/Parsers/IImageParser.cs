using UnityEngine;

namespace Services.ImageParser.Parsers
{
    public interface IImageParser
    {
        public byte[] Encode(Texture2D texture);

        public Texture2D Decode(byte[] data);
    }
}