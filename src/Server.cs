using System.Net;
using System.Net.Sockets;
using System.Text;

TcpListener server = new(IPAddress.Any, 4221);
server.Start();

while (true)
{
  TcpClient client = server.AcceptTcpClient();
  Thread clientThread = new(() => HandleClient(client));
  clientThread.Start();
}

static void HandleClient(TcpClient client)
{
  using TcpClient _ = client;
  byte[] bytes = new byte[256];
  string? data = null;
  NetworkStream stream = client.GetStream();

  int i;
  while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
  {
    data = Encoding.ASCII.GetString(bytes, 0, i);

    byte[] msg = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n\r\n");
    stream.Write(msg, 0, msg.Length);
    stream.Flush();
  }
}
