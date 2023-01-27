using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace ASP.NET_Core_WebSocket_Server;

public class ExampleController: ControllerBase
{
    [HttpGet("/echo")]
    public async Task GetEcho()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await Echo(webSocket);
        }
        else HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
    
    [HttpGet("/time")]
    public async Task GetTime()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await Time(webSocket);
        }
        else HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
    
    private static async Task Echo(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!receiveResult.CloseStatus.HasValue)
        {
            var clientMessageString = Encoding.Default.GetString(buffer, 0, receiveResult.Count);
            var serverResponseString = string.Concat("Your message was: ", clientMessageString);
            var serverResponse = Encoding.Default.GetBytes(serverResponseString);
        
            await webSocket.SendAsync(
                new ArraySegment<byte>(serverResponse, 0, serverResponse.Length),
                receiveResult.MessageType,
                receiveResult.EndOfMessage,
                CancellationToken.None);

            receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }
    
    private static async Task Time(WebSocket webSocket)
    {
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(3));
        
        var buffer = new byte[1024 * 4];
        var receiveResult = webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);
        
        while (await timer.WaitForNextTickAsync())
        {
            if (receiveResult.IsCompleted)
            {
                Console.WriteLine(string.Concat("Received message: ", Encoding.Default.GetString(buffer, 0, receiveResult.Result.Count)));
                if (receiveResult.Result.CloseStatus.HasValue) break;
                
                receiveResult = webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            
            var serverMessageString = DateTime.Now.ToLongTimeString();
            var serverMessage = Encoding.Default.GetBytes(serverMessageString);
        
            await webSocket.SendAsync(
                new ArraySegment<byte>(serverMessage, 0, serverMessage.Length),
                WebSocketMessageType.Text,
                WebSocketMessageFlags.EndOfMessage,
                CancellationToken.None);
        }
        
        await webSocket.CloseAsync(
            WebSocketCloseStatus.NormalClosure,
            "Normal Closure",
            CancellationToken.None);
    }
}