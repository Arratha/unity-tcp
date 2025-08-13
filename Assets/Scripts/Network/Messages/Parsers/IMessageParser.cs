using Network.Messages.Models;

namespace Network.Messages.Parsers
{
    public interface IMessageParser
    {
        public byte[] Serialize(IMessage message);

        public IMessage Deserialize(byte[] data);
    }
}