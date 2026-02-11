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
    if (!requestHeaders.TryGetValues(HttpConstants.HttpHeaderNames.AcceptEncoding, out var values))
      return (null, null);

    var acceptedEncodings = string.Join(",", values).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    if (acceptedEncodings.Contains(HttpConstants.HttpHeaderValues.BrotliEncoding, StringComparer.OrdinalIgnoreCase))
      return (Compress(content, (s) => new BrotliStream(s, CompressionLevel.Optimal)), HttpConstants.HttpHeaderValues.BrotliEncoding);

    if (acceptedEncodings.Contains(HttpConstants.HttpHeaderValues.GzipEncoding, StringComparer.OrdinalIgnoreCase))
      return (Compress(content, (s) => new GZipStream(s, CompressionLevel.Optimal)), HttpConstants.HttpHeaderValues.GzipEncoding);

    if (acceptedEncodings.Contains(HttpConstants.HttpHeaderValues.DeflateEncoding, StringComparer.OrdinalIgnoreCase))
      return (Compress(content, (s) => new DeflateStream(s, CompressionLevel.Optimal)), HttpConstants.HttpHeaderValues.DeflateEncoding);

    return (null, null);
  }

  private static byte[] Compress(string content, Func<Stream, Stream> streamFactory)
  {
    using var outputStream = new MemoryStream();
    using (var compressionStream = streamFactory(outputStream))
    {
      using var writer = new StreamWriter(compressionStream);
      writer.Write(content);
    }
    return outputStream.ToArray();
  }
}
