using System.Text;

namespace codecrafters_http_server.src.Helpers;

public static class HttpRequestParser
{
  private const string HeaderSeparator = "\r\n\r\n";
  private const string ContentLengthHeader = "Content-Length:";
  private const int HeaderSeparatorLength = 4; // \r\n\r\n

  public static int GetFirstRequestLength(string data)
  {
    var headerEnd = data.IndexOf(HeaderSeparator, StringComparison.Ordinal);
    if (headerEnd < 0)
      return -1;

    var headersLength = headerEnd + HeaderSeparatorLength;
    var bodyStart = headersLength;

    // Check for Content-Length to know total request size
    var headers = data[..headerEnd];
    var clIdx = headers.IndexOf(ContentLengthHeader, StringComparison.OrdinalIgnoreCase);
    if (clIdx >= 0)
    {
      var valueStart = clIdx + ContentLengthHeader.Length;
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
}
