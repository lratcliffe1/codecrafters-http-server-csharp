using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;

namespace codecrafters_http_server.src.Helpers;

public static class ResponseHelper
{
  private const string GzipEncoding = "gzip";

  public static HttpContentHeaders CreateContentHeaders(string body, string contentType, string? contentEncoding = null)
  {
    var contentLength = Encoding.UTF8.GetByteCount(body);
    return CreateContentHeaders(contentLength, contentType, contentEncoding);
  }

  public static HttpContentHeaders CreateContentHeaders(byte[] bodyBytes, string contentType, string? contentEncoding = null)
  {
    return CreateContentHeaders(bodyBytes.Length, contentType, contentEncoding);
  }

  private static HttpContentHeaders CreateContentHeaders(int contentLength, string contentType, string? contentEncoding)
  {
    var headers = new ByteArrayContent([]).Headers;
    headers.Clear();
    headers.Add("Content-Type", contentType);
    headers.Add("Content-Length", contentLength.ToString());
    if (contentEncoding != null)
      headers.Add("Content-Encoding", contentEncoding);
    return headers;
  }

  public static (byte[]? bodyBytes, string? contentEncoding) TryCompressIfAccepted(string content, HttpHeaders requestHeaders)
  {
    if (requestHeaders.TryGetValues("Accept-Encoding", out var acceptEncoding) && acceptEncoding.Contains(GzipEncoding))
    {
      return (GzipCompress(content), GzipEncoding);
    }
    return (null, null);
  }

  private static byte[] GzipCompress(string content)
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
