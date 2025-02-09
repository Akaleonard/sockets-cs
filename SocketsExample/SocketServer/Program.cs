using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketServer
{
    internal class Program
    {
        private static readonly List<Socket> _clients = new List<Socket>();

        static async Task Main(string[] args)
        {
            var hostname = Dns.GetHostName();
            IPHostEntry localhost = await Dns.GetHostEntryAsync(hostname);
            IPAddress localIpAddress = localhost.AddressList[0];
            IPEndPoint endPoint = new(localIpAddress, 11_000);

            // Create listener
            using Socket listener = new(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Bind endpoint to listener
            listener.Bind(endPoint);
            listener.Listen(10);

            while (true)
            {
                var handler = await listener.AcceptAsync(); // Accept incoming request
                _clients.Add(handler);
                _ = HandleClientCommunication(handler);
            }
        }

        static async Task HandleClientCommunication(Socket handler)
        {
            try
            {
                while (true)
                {
                    var buffer = new byte[1024];
                    int received;

                    try
                    {
                        // Receive message
                        received = await handler.ReceiveAsync(buffer, SocketFlags.None);

                        if (received == 0)
                        {
                            // Client has disconnected, stop the loop
                            break;
                        }

                        var response = Encoding.UTF8.GetString(buffer, 0, received);
                        Console.WriteLine($"Server received message: \"{response}\"");

                        // Broadcast message to all other clients
                        foreach (var client in _clients)
                        {
                            Console.WriteLine($"Comparing client: {client} with handler: {handler}");
                            if (client != handler && client.Connected)
                            {
                                var messageBytes = Encoding.UTF8.GetBytes(response);
                                await client.SendAsync(messageBytes, SocketFlags.None);
                                Console.WriteLine($"Sent message to client: \"{response}\"");
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error receiving message: {ex.Message}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client communication: {ex.Message}");
            }
            finally
            {
                // Clean up
                _clients.Remove(handler);
                handler.Close();
                Console.WriteLine("Client disconnected and removed from list.");
            }
        }
    }
}
