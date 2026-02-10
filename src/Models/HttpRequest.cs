using System.Net.Http.Headers;

namespace codecrafters_http_server.src.Models;

public class HttpRequest
{
  private const string HeaderBodySeparator = "\r\n\r\n";
  private const string HeaderLineSeparator = "\r\n";
  private const string HeaderValueSeparator = ": ";
  private const string HttpVersionPrefix = "HTTP/";
  private const string DefaultHttpVersion = "1.1";

  public HttpMethod HttpMethod { get; init; }
  public string HttpTarget { get; init; }
  public Version HttpVersion { get; init; }
  public HttpHeaders HttpHeaders { get; init; }
  public string RequestBody { get; init; }

  public HttpRequest(string request)
  {
    if (string.IsNullOrWhiteSpace(request))
      throw new ArgumentException("Request cannot be empty.", nameof(request));

    var headAndBody = request.Split(HeaderBodySeparator, 2, StringSplitOptions.None);
    var head = headAndBody[0];
    var body = headAndBody.Length > 1 ? headAndBody[1] : "";

    var requestLines = head.Split(HeaderLineSeparator, StringSplitOptions.RemoveEmptyEntries);
    if (requestLines.Length == 0)
      throw new ArgumentException("Invalid request: missing request line.", nameof(request));

    var requestLineParts = requestLines[0].Split(' ', 3);
    if (requestLineParts.Length < 3)
      throw new ArgumentException("Invalid request line: expected 'METHOD TARGET VERSION'.", nameof(request));

    var method = requestLineParts[0];
    var target = requestLineParts[1];
    var versionString = requestLineParts[2];
    var version = versionString.StartsWith(HttpVersionPrefix, StringComparison.OrdinalIgnoreCase)
      ? versionString[HttpVersionPrefix.Length..]
      : versionString.Split('/').LastOrDefault() ?? DefaultHttpVersion;

    HttpMethod = GetHttpMethod(method);
    HttpTarget = target;
    HttpVersion = GetHttpVersion(version);
    HttpHeaders = GetHttpHeaders(requestLines);
    RequestBody = body;
  }

  private static HttpMethod GetHttpMethod(string method)
  {
    return method switch
    {
      "GET" => HttpMethod.Get,
      "POST" => HttpMethod.Post,
      "PUT" => HttpMethod.Put,
      "DELETE" => HttpMethod.Delete,
      "PATCH" => HttpMethod.Patch,
      _ => throw new NotImplementedException($"Unsupported HTTP method: {method}"),
    };
  }

  private static Version GetHttpVersion(string version)
  {
    return new Version(version);
  }

  private static HttpRequestHeaders GetHttpHeaders(string[] requestLines)
  {
    var headers = new HttpRequestMessage().Headers;
    for (var i = 1; i < requestLines.Length; i++)
    {
      var line = requestLines[i];
      var colonIndex = line.IndexOf(HeaderValueSeparator, StringComparison.Ordinal);
      if (colonIndex > 0)
      {
        var headerName = line[..colonIndex];
        var headerValue = line[(colonIndex + HeaderValueSeparator.Length)..];
        headers.TryAddWithoutValidation(headerName, headerValue);
      }
    }

    return headers;
  }
}
