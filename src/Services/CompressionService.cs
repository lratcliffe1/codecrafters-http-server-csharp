using System.IO.Compression;
using System.Net.Http.Headers;
using codecrafters_http_server.src.Constants;

namespace codecrafters_http_server.src.Services;

public interface ICompressionService
{
  (byte[]? bodyBytes, string? contentEncoding) TryCompressIfAccepted(string content, HttpHeaders requestHeaders);
}

public class CompressionService : ICompressionService
{
  public (byte[]? bodyBytes, string? contentEncoding) TryCompressIfAccepted(string content, HttpHeaders requestHeaders)
  {
    if (requestHeaders.TryGetValues(HttpConstants.HttpHeaderNames.AcceptEncoding, out var acceptEncoding) && acceptEncoding.Contains(HttpConstants.HttpHeaderValues.GzipEncoding))
    {
      return (GzipCompress(content), HttpConstants.HttpHeaderValues.GzipEncoding);
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
