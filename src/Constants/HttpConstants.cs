namespace codecrafters_http_server.src.Constants;

public static class HttpConstants
{
  public static class HttpHeaderNames
  {
    public const string Connection = "Connection";
    public const string ContentType = "Content-Type";
    public const string ContentLength = "Content-Length";
    public const string ContentEncoding = "Content-Encoding";
    public const string UserAgent = "User-Agent";
    public const string AcceptEncoding = "Accept-Encoding";
  }

  public static class HttpHeaderValues
  {
    public const string ConnectionClose = "close";
    public const string GzipEncoding = "gzip";
  }

  public static class ContentTypes
  {
    public const string TextPlain = "text/plain";
    public const string ApplicationOctetStream = "application/octet-stream";
  }

  public static class HttpProtocol
  {
    public const string HeaderBodySeparator = "\r\n\r\n";
    public const string HeaderLineSeparator = "\r\n";
    public const string HeaderValueSeparator = ": ";
    public const string HttpVersionPrefix = "HTTP/";
    public const string DefaultHttpVersion = "1.1";
    public const int HeaderSeparatorLength = 4; // \r\n\r\n
    public const string ContentLengthHeaderPrefix = "Content-Length:";
  }

  public static class ErrorMessages
  {
    public const string BadRequestMessage = "Bad Request";
    public const string RequestCannotBeEmpty = "Request cannot be empty.";
    public const string InvalidRequestMissingRequestLine = "Invalid request: missing request line.";
    public const string InvalidRequestLine = "Invalid request line: expected 'METHOD TARGET VERSION'.";
  }

  public static class HttpMethods
  {
    public const string Get = "GET";
    public const string Post = "POST";
    public const string Put = "PUT";
    public const string Delete = "DELETE";
    public const string Patch = "PATCH";
  }
}
