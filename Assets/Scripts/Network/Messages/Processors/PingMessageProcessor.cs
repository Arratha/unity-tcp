using Network.Client;
using Network.Messages.Models;

namespace Network.Messages.Processors
{
    //Ping message is a system message
    //It may be logged, but it is not meant to be processed
    public class PingMessageProcessor : IMessageProcessor<PingMessage>
    {
        public void Process(ClientId client, PingMessage message)
        {
            
        }
    }
}