using System.Net;
using System.Net.Sockets;
using System.Text;
using codecrafters_http_server.src;
using codecrafters_http_server.src.Helpers;
using codecrafters_http_server.src.Models;

const int Port = 4221;
const int BufferSize = 1024;

string? filesDirectory = null;
for (var i = 0; i < args.Length - 1; i++)
  if (args[i] == "--directory")
    filesDirectory = args[++i];

if (filesDirectory != null)
  ResponseParser.FilesDirectory = filesDirectory;

var server = new TcpListener(IPAddress.Any, Port);
server.Start();

while (true)
{
  try
  {
    var client = server.AcceptTcpClient();
    var clientThread = new Thread(() => HandleClient(client))
    {
      IsBackground = true
    };
    clientThread.Start();
  }
  catch (Exception ex)
  {
    Console.Error.WriteLine($"Error accepting client: {ex.Message}");
  }
}

static void HandleClient(TcpClient client)
{
  try
  {
    using var _ = client;
    using var stream = client.GetStream();
    var buffer = new byte[BufferSize];
    var requestBuilder = new StringBuilder();

    int bytesRead;
    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
    {
      HttpRequest? httpRequest = null;
      requestBuilder.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));

      while (true)
      {
        var data = requestBuilder.ToString();
        var requestLength = HttpRequestParser.GetFirstRequestLength(data);
        if (requestLength < 0)
          break;

        var requestData = data[..requestLength];
        try
        {
          httpRequest = new HttpRequest(requestData);
          var responseBytes = ResponseParser.Parse(httpRequest).ToResponseBytes();

          stream.Write(responseBytes, 0, responseBytes.Length);
          stream.Flush();
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine($"Error processing request: {ex.Message}");

          var emptyHeaders = new ByteArrayContent([]).Headers;
          emptyHeaders.Clear();

          var errorResponse = new HttpResponse(new Version(1, 1), emptyHeaders, "Bad Request", HttpStatusCode.BadRequest);
          var errorBytes = errorResponse.ToResponseBytes();

          stream.Write(errorBytes, 0, errorBytes.Length);
          stream.Flush();
        }

        requestBuilder.Remove(0, requestLength);
      }

      if (httpRequest == null || (httpRequest.HttpHeaders.TryGetValues("Connection", out var connectionValues) == true && connectionValues?.Contains("close") == true))
        break;
    }
  }
  catch (Exception ex)
  {
    Console.Error.WriteLine($"Error handling client: {ex.Message}");
  }
}
