using System.Net.WebSockets;
using System.Text;

namespace WebSocket_Client
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            
            Console.CancelKeyPress += (_, _) =>
            {
                Console.WriteLine("Cancel event triggered");
                cancellationTokenSource.Cancel();
            };
            
            var targetServerAddress = "ws://127.0.0.1:5080/echo";
            if (args.Any()) targetServerAddress = args[0];
            
            using var socket = new ClientWebSocket();
            try
            {
                await socket.ConnectAsync(new Uri(targetServerAddress), cancellationTokenSource.Token);
                var listenTask = Listen(socket, cancellationTokenSource.Token);
                var sendTask = Send(socket, cancellationTokenSource.Token);
                Task.WaitAll(listenTask, sendTask);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        
        private static async Task Listen(ClientWebSocket socket, CancellationToken cancellationToken)
        {
            do
            {
                var buffer = new ArraySegment<byte>(new byte[2048]);
                WebSocketReceiveResult receiveResult;
                using var stream = new MemoryStream();
                do
                {
                    receiveResult = await socket.ReceiveAsync(buffer, cancellationToken);
                    stream.Write(buffer.Array, buffer.Offset, receiveResult.Count);
                } while (!receiveResult.EndOfMessage);

                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }

                stream.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(stream, Encoding.Default);
                Console.WriteLine(await reader.ReadToEndAsync());
                
            } while (!cancellationToken.IsCancellationRequested);
        }
        
        private static async Task Send(ClientWebSocket socket, CancellationToken cancellationToken)
        {
            do
            {
                Console.WriteLine("Type text that you want to use to server...");
                var text = Console.ReadLine() ?? string.Empty;
                await socket.SendAsync(Encoding.Default.GetBytes(text), WebSocketMessageType.Text, true, cancellationToken);
            } while (!cancellationToken.IsCancellationRequested);
        }
    }
}