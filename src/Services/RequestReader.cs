using System.Text;
using codecrafters_http_server.src.Constants;
using codecrafters_http_server.src.Models;

namespace codecrafters_http_server.src.Services;

public interface IRequestReader
{
  bool TryReadFromStream(Stream stream, StringBuilder requestBuilder);
  HttpRequest? TryParseCompleteRequest(StringBuilder requestBuilder);
  bool ShouldCloseConnection(HttpRequest? request);
}

public class RequestReader : IRequestReader
{
  private const int BufferSize = 1024;

  public bool TryReadFromStream(Stream stream, StringBuilder requestBuilder)
  {
    var buffer = new byte[BufferSize];
    var bytesRead = stream.Read(buffer, 0, buffer.Length);

    if (bytesRead == 0)
      return false; // Stream closed

    requestBuilder.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
    return true;
  }

  public HttpRequest? TryParseCompleteRequest(StringBuilder requestBuilder)
  {
    var data = requestBuilder.ToString();
    var requestLength = GetFirstRequestLength(data);

    if (requestLength < 0)
      return null; // Need more data

    var requestData = data[..requestLength];
    try
    {
      var request = new HttpRequest(requestData);
      requestBuilder.Remove(0, requestLength);
      return request;
    }
    catch
    {
      return null;
    }
  }

  public bool ShouldCloseConnection(HttpRequest? request)
  {
    if (request == null)
      return true;

    return request.HttpHeaders.TryGetValues(HttpConstants.HttpHeaderNames.Connection, out var connectionValues) &&
      connectionValues?.Contains(HttpConstants.HttpHeaderValues.ConnectionClose) == true;
  }

  private static int GetFirstRequestLength(string data)
  {
    var headerEnd = data.IndexOf(HttpConstants.HttpProtocol.HeaderBodySeparator, StringComparison.Ordinal);
    if (headerEnd < 0)
      return -1;

    var headersLength = headerEnd + HttpConstants.HttpProtocol.HeaderSeparatorLength;

    // Check for Content-Length to know total request size
    var headers = data[..headerEnd];
    var clIdx = headers.IndexOf(HttpConstants.HttpProtocol.ContentLengthHeaderPrefix, StringComparison.OrdinalIgnoreCase);
    if (clIdx >= 0)
    {
      var valueStart = clIdx + HttpConstants.HttpProtocol.ContentLengthHeaderPrefix.Length;
      var valueEnd = headers.IndexOf('\r', valueStart);
      if (valueEnd < 0) valueEnd = headers.Length;
      var value = headers[valueStart..valueEnd].Trim();
      if (int.TryParse(value, out var contentLength))
      {
        var totalLength = headersLength + contentLength;
        if (data.Length < totalLength)
          return -1; // need more data
        return totalLength;
      }
    }

    return headersLength;
  }
}
