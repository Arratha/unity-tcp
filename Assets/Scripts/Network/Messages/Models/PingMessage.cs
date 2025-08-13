using System;
using MessagePack;

namespace Network.Messages.Models
{
    [MessagePackObject]
    public class PingMessage : IMessage
    {
        [Key(0)] public DateTime time { get; set; }
    }
}