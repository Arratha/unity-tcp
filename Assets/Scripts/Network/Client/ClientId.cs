using System.Net;
using System.Net.Sockets;

namespace Network.Client
{
    public class ClientId
    {
        private readonly EndPoint _endPoint;
        
        public ClientId(TcpClient client)
        {
            _endPoint = client.Client.RemoteEndPoint;
        }

        public override bool Equals(object obj)
        {
            if (obj is not ClientId clientId)
            {
                return false;
            }

            return _endPoint.Equals(clientId._endPoint);
        }
        
        public override int GetHashCode()
        {
            return _endPoint.GetHashCode();
        }
    }
}