using System.Net;
using System.Net.Sockets;
using System.Text;
using codecrafters_http_server.src;
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

    while (true)
    {
      var data = requestBuilder.ToString();
      var requestLength = GetFirstRequestLength(data);
      if (requestLength < 0)
        break;

      var requestData = data[..requestLength];
      var httpRequest = new HttpRequest(requestData);

      var responseBytes = ResponseParser.Parse(httpRequest).ToResponseBytes();

      stream.Write(responseBytes, 0, responseBytes.Length);
      stream.Flush();

      requestBuilder.Remove(0, requestLength);
    }
  }
}

static int GetFirstRequestLength(string data)
{
  var headerEnd = data.IndexOf("\r\n\r\n", StringComparison.Ordinal);
  if (headerEnd < 0)
    return -1;

  var headersLength = headerEnd + 4; // include \r\n\r\n
  var bodyStart = headersLength;

  // Check for Content-Length to know total request size
  var headers = data[..headerEnd];
  const string contentLengthPrefix = "Content-Length:";
  var clIdx = headers.IndexOf(contentLengthPrefix, StringComparison.OrdinalIgnoreCase);
  if (clIdx >= 0)
  {
    var valueStart = clIdx + contentLengthPrefix.Length;
    var valueEnd = headers.IndexOf('\r', valueStart);
    if (valueEnd < 0) valueEnd = headers.Length;
    var value = headers[valueStart..valueEnd].Trim();
    if (int.TryParse(value, out var contentLength))
    {
      var totalLength = bodyStart + contentLength;
      if (data.Length < totalLength)
        return -1; // need more data
      return totalLength;
    }
  }

  return headersLength;
}
