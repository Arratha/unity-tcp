namespace Network.Messages.Models
{
    [MessagePack.Union(0, typeof(PingMessage))]
    [MessagePack.Union(1, typeof(ImageMessage))]
    public interface IMessage
    {
        
    }
}