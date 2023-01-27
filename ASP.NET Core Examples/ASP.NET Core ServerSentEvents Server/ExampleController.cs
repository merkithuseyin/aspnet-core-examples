using Microsoft.AspNetCore.Mvc;

namespace ASP.NET_Core_ServerSentEvents_Server;

public class ExampleController: ControllerBase
{
    [HttpGet("/time")]
    public async Task Get(CancellationToken cancellationToken)
    {
        Response.Headers.Add("Content-Type", "text/event-stream");
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(3));

        while (!cancellationToken.IsCancellationRequested && await timer.WaitForNextTickAsync(cancellationToken))
        {
            await Response.WriteAsync($"{DateTime.Now}\r\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }
}