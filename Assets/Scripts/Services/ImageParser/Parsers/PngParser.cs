using UnityEngine;

namespace Services.ImageParser.Parsers
{
    public class PngParser : IImageParser
    {
        public byte[] Encode(Texture2D texture)
        {
            var tempTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA4444, false);

            tempTexture.SetPixels(texture.GetPixels());
            tempTexture.Apply();

            var data = tempTexture.EncodeToPNG();
            Object.Destroy(tempTexture);
            
            return data;
        }

        public Texture2D Decode(byte[] data)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA4444, false);
            texture.LoadImage(data);
            return texture;
        }
    }
}