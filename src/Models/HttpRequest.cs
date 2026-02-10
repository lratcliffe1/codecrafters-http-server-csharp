using System.Net.Http.Headers;

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
      throw new ArgumentException("Request cannot be empty.", nameof(request));

    var headAndBody = request.Split("\r\n\r\n", 2, StringSplitOptions.None);
    var head = headAndBody[0];
    var body = headAndBody.Length > 1 ? headAndBody[1] : "";

    var requestLines = head.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
    if (requestLines.Length == 0)
      throw new ArgumentException("Invalid request: missing request line.", nameof(request));

    var requestLineParts = requestLines[0].Split(' ', 3);
    if (requestLineParts.Length < 3)
      throw new ArgumentException("Invalid request line: expected 'METHOD TARGET VERSION'.", nameof(request));

    var method = requestLineParts[0];
    var target = requestLineParts[1];
    var version = requestLineParts[2].Split('/').LastOrDefault() ?? "1.1";

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
      var colon = line.IndexOf(": ", StringComparison.Ordinal);
      if (colon > 0)
        headers.TryAddWithoutValidation(line[..colon], line[(colon + 2)..]);
    }

    return headers;
  }
}
