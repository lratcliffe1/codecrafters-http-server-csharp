using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace codecrafters_http_server.src.Helpers;

public static class ResponseHelper
{
  private const string GzipEncoding = "gzip";

  public static HttpHeaders CreateContentHeaders(string body, string contentType, HttpHeaders requestHeaders, string? contentEncoding = null)
  {
    var contentLength = Encoding.UTF8.GetByteCount(body);
    return CreateContentHeaders(contentLength, contentType, requestHeaders, contentEncoding);
  }

  public static HttpHeaders CreateContentHeaders(byte[] bodyBytes, string contentType, HttpHeaders requestHeaders, string? contentEncoding = null)
  {
    return CreateContentHeaders(bodyBytes.Length, contentType, requestHeaders, contentEncoding);
  }

  private static HttpHeaders CreateContentHeaders(int contentLength, string contentType, HttpHeaders requestHeaders, string? contentEncoding = null)
  {
    var contentHeaders = new ByteArrayContent([]).Headers;
    contentHeaders.Clear();
    contentHeaders.Add("Content-Type", contentType);
    contentHeaders.Add("Content-Length", contentLength.ToString());

    if (!string.IsNullOrEmpty(contentEncoding))
      contentHeaders.Add("Content-Encoding", contentEncoding);

    var responseHeaders = new HttpResponseMessage().Headers;
    if (requestHeaders.TryGetValues("Connection", out var connectionValues))
      responseHeaders.TryAddWithoutValidation("Connection", connectionValues);

    return new CombinedHeadersWrapper(contentHeaders, responseHeaders);
  }

  private class CombinedHeadersWrapper : HttpHeaders, IEnumerable<KeyValuePair<string, IEnumerable<string>>>
  {
    private readonly List<KeyValuePair<string, IEnumerable<string>>> _headers;

    public CombinedHeadersWrapper(HttpContentHeaders contentHeaders, HttpResponseHeaders responseHeaders)
    {
      _headers = [.. contentHeaders, .. responseHeaders];
    }

    IEnumerator<KeyValuePair<string, IEnumerable<string>>> IEnumerable<KeyValuePair<string, IEnumerable<string>>>.GetEnumerator() => _headers.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _headers.GetEnumerator();
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
