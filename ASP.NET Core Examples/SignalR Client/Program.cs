using Microsoft.AspNetCore.SignalR.Client;

namespace SignalR_Client
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

            var targetHub = "http://localhost:5080/ChatHub";
            if (args.Any()) targetHub = args[0];
            
            var client = new Client(targetHub);
            await client.Connect(cancellationTokenSource.Token);

            var timer = new PeriodicTimer(TimeSpan.FromSeconds(3));
            while (!cancellationTokenSource.Token.IsCancellationRequested && await timer.WaitForNextTickAsync(cancellationTokenSource.Token))
            {
                await client.Send("Hüseyin", "This is my message!", cancellationTokenSource.Token);
            }

            await client.Disconnect();
        }
    }

    internal class Client
    {
        private readonly HubConnection _connection;
        public Client(string targetHub)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(targetHub)
                .Build();

            _connection.Closed += async _ =>
            {
                await Task.Delay(1000);
                await _connection.StartAsync();
            };
        }

        public async Task Connect(CancellationToken cancellationToken)
        {
            _connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                Console.WriteLine($"SERVER: \r\nUser: {user} \r\nMessage: {message}\r\n");
            });

            await _connection.StartAsync(cancellationToken);
            Console.WriteLine("Connection started");
        }

        public async Task Send(string user, string message, CancellationToken cancellationToken)
        {
            Console.WriteLine("Sending message...");
            await _connection.InvokeAsync("SendMessage", user, message, cancellationToken);
        }

        public async Task Disconnect()
        {
            await _connection.StopAsync();
            Console.WriteLine("Connection closed");
        }
    }
}