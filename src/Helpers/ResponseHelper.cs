using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;

namespace codecrafters_http_server.src.Helpers;

public static class ResponseHelper
{
  public static HttpContentHeaders CreateContentHeaders(string body, string contentType, string? contentEncoding = null)
  {
    var headers = new ByteArrayContent([]).Headers;
    headers.Clear();
    headers.Add("Content-Type", contentType);
    headers.Add("Content-Length", Encoding.UTF8.GetByteCount(body).ToString());
    if (contentEncoding != null)
      headers.Add("Content-Encoding", contentEncoding);
    return headers;
  }

  public static HttpContentHeaders CreateContentHeaders(byte[] bodyBytes, string contentType, string? contentEncoding = null)
  {
    var headers = new ByteArrayContent([]).Headers;
    headers.Clear();
    headers.Add("Content-Type", contentType);
    headers.Add("Content-Length", bodyBytes.Length.ToString());
    if (contentEncoding != null)
      headers.Add("Content-Encoding", contentEncoding);
    return headers;
  }

  public static byte[] GzipCompress(string content)
  {
    using var memoryStream = new MemoryStream();
    using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
    {
      using var writer = new StreamWriter(gzipStream);
      writer.Write(content);
      writer.Flush();
    }
    return memoryStream.ToArray();
  }
}
