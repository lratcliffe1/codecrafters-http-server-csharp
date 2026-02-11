using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using codecrafters_http_server.src.Services;

namespace codecrafters_http_server.src;

public class HttpServer(IServiceProvider serviceProvider)
{
  private readonly IServiceProvider _serviceProvider = serviceProvider;

  public async Task Start(int port)
  {
    var tcpListener = new TcpListener(IPAddress.Any, port);
    tcpListener.Start();

    while (true)
    {
      TcpClient? client = null;
      try
      {
        client = await tcpListener.AcceptTcpClientAsync();

        _ = Task.Run(() => HandleClient(client))
          .ContinueWith(task =>
          {
            if (task.IsFaulted)
            {
              Console.Error.WriteLine($"Error in client handler: {task.Exception?.GetBaseException().Message}");
            }
          }, TaskContinuationOptions.OnlyOnFaulted);
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine($"Error accepting client: {ex.Message}");
        client?.Dispose();
      }
    }
  }

  private void HandleClient(TcpClient client)
  {
    using var scope = _serviceProvider.CreateScope();
    var connectionHandler = scope.ServiceProvider.GetRequiredService<IClientConnectionHandler>();
    connectionHandler.HandleClient(client);
  }
}
