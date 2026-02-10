using System.Net;
using System.Net.Sockets;
using System.Text;
using codecrafters_http_server.src;

const int Port = 4221;
const int BufferSize = 1024;

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

    var httpRequest = RequestParser.Parse(data);

    var response = httpRequest.HttpTarget == "/"
      ? "HTTP/1.1 200 OK\r\n\r\n"
      : "HTTP/1.1 404 Not Found\r\n\r\n";

    var msg = Encoding.UTF8.GetBytes(response);
    stream.Write(msg, 0, msg.Length);
    stream.Flush();
    break;
  }
}
