using System.Net.Http.Headers;
using codecrafters_http_server.src.Constants;

namespace codecrafters_http_server.src.Models;

public class HttpRequest
{
  public HttpMethod HttpMethod { get; init; }
  public string HttpTarget { get; init; }
  public Version HttpVersion { get; init; }
  public HttpHeaders HttpHeaders { get; init; }
  public string RequestBody { get; init; }

  public HttpRequest(string request)
  {
    if (string.IsNullOrWhiteSpace(request))
      throw new ArgumentException(HttpConstants.ErrorMessages.RequestCannotBeEmpty, nameof(request));

    var headAndBody = request.Split(HttpConstants.HttpProtocol.HeaderBodySeparator, 2, StringSplitOptions.None);
    var head = headAndBody[0];
    var body = headAndBody.Length > 1 ? headAndBody[1] : "";

    var requestLines = head.Split(HttpConstants.HttpProtocol.HeaderLineSeparator, StringSplitOptions.RemoveEmptyEntries);
    if (requestLines.Length == 0)
      throw new ArgumentException(HttpConstants.ErrorMessages.InvalidRequestMissingRequestLine, nameof(request));

    var requestLineParts = requestLines[0].Split(' ', 3);
    if (requestLineParts.Length < 3)
      throw new ArgumentException(HttpConstants.ErrorMessages.InvalidRequestLine, nameof(request));

    var method = requestLineParts[0];
    var target = requestLineParts[1];
    var versionString = requestLineParts[2];
    var version = versionString.StartsWith(HttpConstants.HttpProtocol.HttpVersionPrefix, StringComparison.OrdinalIgnoreCase)
      ? versionString[HttpConstants.HttpProtocol.HttpVersionPrefix.Length..]
      : versionString.Split('/').LastOrDefault() ?? HttpConstants.HttpProtocol.DefaultHttpVersion;

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
      HttpConstants.HttpMethods.Get => HttpMethod.Get,
      HttpConstants.HttpMethods.Post => HttpMethod.Post,
      HttpConstants.HttpMethods.Put => HttpMethod.Put,
      HttpConstants.HttpMethods.Delete => HttpMethod.Delete,
      HttpConstants.HttpMethods.Patch => HttpMethod.Patch,
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
      var colonIndex = line.IndexOf(HttpConstants.HttpProtocol.HeaderValueSeparator, StringComparison.Ordinal);
      if (colonIndex > 0)
      {
        var headerName = line[..colonIndex];
        var headerValue = line[(colonIndex + HttpConstants.HttpProtocol.HeaderValueSeparator.Length)..];
        headers.TryAddWithoutValidation(headerName, headerValue);
      }
    }

    return headers;
  }
}
