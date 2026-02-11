using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using codecrafters_http_server.src.Constants;

namespace codecrafters_http_server.src.Services;

public interface IResponseHeaderBuilder
{
  HttpHeaders BuildContentHeaders(int contentLength, string contentType, HttpHeaders requestHeaders, string? contentEncoding = null);
  HttpHeaders BuildContentHeaders(string body, string contentType, HttpHeaders requestHeaders, string? contentEncoding = null);
  HttpHeaders BuildContentHeaders(byte[] bodyBytes, string contentType, HttpHeaders requestHeaders, string? contentEncoding = null);
}

public class ResponseHeaderBuilder : IResponseHeaderBuilder
{
  public HttpHeaders BuildContentHeaders(string body, string contentType, HttpHeaders requestHeaders, string? contentEncoding = null)
  {
    var contentLength = Encoding.UTF8.GetByteCount(body);
    return BuildContentHeaders(contentLength, contentType, requestHeaders, contentEncoding);
  }

  public HttpHeaders BuildContentHeaders(byte[] bodyBytes, string contentType, HttpHeaders requestHeaders, string? contentEncoding = null)
  {
    return BuildContentHeaders(bodyBytes.Length, contentType, requestHeaders, contentEncoding);
  }

  public HttpHeaders BuildContentHeaders(int contentLength, string contentType, HttpHeaders requestHeaders, string? contentEncoding = null)
  {
    var contentHeaders = new ByteArrayContent([]).Headers;
    contentHeaders.Clear();
    contentHeaders.Add(HttpConstants.HttpHeaderNames.ContentType, contentType);
    contentHeaders.Add(HttpConstants.HttpHeaderNames.ContentLength, contentLength.ToString());

    if (!string.IsNullOrEmpty(contentEncoding))
      contentHeaders.Add(HttpConstants.HttpHeaderNames.ContentEncoding, contentEncoding);

    var responseHeaders = new HttpResponseMessage().Headers;
    if (requestHeaders.TryGetValues(HttpConstants.HttpHeaderNames.Connection, out var connectionValues))
      responseHeaders.TryAddWithoutValidation(HttpConstants.HttpHeaderNames.Connection, connectionValues);

    return new CombinedHeadersWrapper(contentHeaders, responseHeaders);
  }

  private class CombinedHeadersWrapper(HttpContentHeaders contentHeaders, HttpResponseHeaders responseHeaders) : HttpHeaders, IEnumerable<KeyValuePair<string, IEnumerable<string>>>
  {
    private readonly List<KeyValuePair<string, IEnumerable<string>>> _headers = [.. contentHeaders, .. responseHeaders];

    IEnumerator<KeyValuePair<string, IEnumerable<string>>> IEnumerable<KeyValuePair<string, IEnumerable<string>>>.GetEnumerator() => _headers.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _headers.GetEnumerator();
  }
}
