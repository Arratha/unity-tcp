using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Extensions
{
    public static class NetworkStreamExtension
    {
        public static async Task ReadExactlyAsync(this NetworkStream stream, byte[] buffer, int offset, int count,
            CancellationToken token)
        {
            while (count > 0 && !token.IsCancellationRequested)
            {
                var read = await stream.ReadAsync(buffer, offset, count, token);
                if (read == 0)
                {
                    throw new EndOfStreamException("Unexpected end of stream");
                }
                offset += read;
                count -= read;
            }
        
            if (count > 0 && token.IsCancellationRequested)
            {
                throw new OperationCanceledException(token);
            }
        }
    }
}