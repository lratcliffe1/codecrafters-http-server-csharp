using System.Net;
using System.Net.Sockets;
using System.Text;
using codecrafters_http_server.src;

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
  var client = server.AcceptTcpClient();
  var clientThread = new Thread(() => HandleClient(client));
  clientThread.Start();
}

static void HandleClient(TcpClient client)
{
  using var _ = client;
  using var stream = client.GetStream();
  var buffer = new byte[BufferSize];
  var requestBuilder = new StringBuilder();

  int bytesRead;
  while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
  {
    requestBuilder.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
    var data = requestBuilder.ToString();
    if (!data.Contains("\r\n\r\n"))
      continue;

    var httpRequest = new HttpRequest(data);

    var responseBytes = ResponseParser.Parse(httpRequest).ToResponseBytes();

    stream.Write(responseBytes, 0, responseBytes.Length);
    stream.Flush();
    break;
  }
}
