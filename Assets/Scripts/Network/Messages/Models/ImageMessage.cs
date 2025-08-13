using MessagePack;
using Services.ImageParser.Factory;

namespace Network.Messages.Models
{
    [MessagePackObject]
    public class ImageMessage : IMessage
    {
        [Key(0)] public ImageType type { get; set; }

        [Key(1)] public byte[] data { get; set; }
    }
}