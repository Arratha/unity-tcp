using MessagePack;
using Network.Messages.Models;

namespace Network.Messages.Parsers
{
    public class MessageParser : IMessageParser
    {
        public byte[] Serialize(IMessage message)
        {
            return MessagePackSerializer.Serialize(message);
        }

        public IMessage Deserialize(byte[] data)
        {
            return MessagePackSerializer.Deserialize<IMessage>(data);
        }
    }
}