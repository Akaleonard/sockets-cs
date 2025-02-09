using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketsClients
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var hostname = Dns.GetHostName();
            IPHostEntry localhost = await Dns.GetHostEntryAsync(hostname);
            IPAddress localIpAddress = localhost.AddressList[0];
            IPEndPoint endPoint = new(localIpAddress, 11_000);

            using Socket client = new(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            await client.ConnectAsync(endPoint);
            var receiveTask = ReceiveMessagesAsync(client); // Start receiving messages in parallel.

            while (true)
            {
                // Send message
                var message = Console.ReadLine();
                if (message == null) break;

                var messageBytes = Encoding.UTF8.GetBytes(message);
                await client.SendAsync(messageBytes, SocketFlags.None);
                Console.WriteLine($"Socket client sent message: \"{message}\"");
            }

            client.Shutdown(SocketShutdown.Both);
        }

        static async Task ReceiveMessagesAsync(Socket client)
        {
            var buffer = new byte[1024];
            while (true)
            {
                try
                {
                    // Receive message asynchronously
                    var received = await client.ReceiveAsync(buffer, SocketFlags.None);
                    if (received == 0) break;

                    var response = Encoding.UTF8.GetString(buffer, 0, received);
                    Console.WriteLine($"Socket client received message: \"{response}\"");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiving message: {ex.Message}");
                    break;
                }
            }
        }
    }
}
