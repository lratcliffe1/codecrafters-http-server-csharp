using System.Net.Http.Headers;

namespace codecrafters_http_server.src;

public static class RequestParser
{
  public static HttpRequest Parse(string request)
  {
    if (string.IsNullOrWhiteSpace(request))
      throw new ArgumentException("Request cannot be empty.", nameof(request));

    var headAndBody = request.Split("\r\n\r\n", 2, StringSplitOptions.None);
    var head = headAndBody[0];
    var body = headAndBody.Length > 1 ? headAndBody[1] : "";

    var requestLines = head.Split("\r\n");
    var requestLine = requestLines[0];
    var requestLineParts = requestLine.Split(' ', 3);
    if (requestLineParts.Length < 3)
      throw new ArgumentException("Invalid request line: expected 'METHOD TARGET VERSION'.", nameof(request));

    var method = requestLineParts[0];
    var target = requestLineParts[1];
    var version = requestLineParts[2].Split('/').Last();

    return new HttpRequest(
      GetHttpMethod(method),
      target,
      GetHttpVersion(version),
      GetHttpHeaders(requestLines),
      body);
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
