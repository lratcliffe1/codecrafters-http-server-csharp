using System.Net;
using System.Net.Http.Headers;

namespace codecrafters_http_server.src;

public class HttpResponse(Version httpVersion, HttpHeaders httpHeaders, string requestBody, HttpStatusCode httpStatusCode)
{
  public Version HttpVersion { get; init; } = httpVersion;
  public HttpStatusCode HttpStatusCode { get; init; } = httpStatusCode;
  public HttpHeaders HttpHeaders { get; init; } = httpHeaders;
  public string RequestBody { get; init; } = requestBody;

  public HttpResponse(HttpRequest request, HttpStatusCode httpStatusCode) : this(request.HttpVersion, request.HttpHeaders, request.RequestBody, httpStatusCode) { }

  public string ToResponseString()
  {
    var headerLines = string.Join("\r\n", HttpHeaders.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"));
    var reasonPhrase = GetReasonPhrase(HttpStatusCode);
    return $"HTTP/{HttpVersion} {(int)HttpStatusCode} {reasonPhrase}\r\n{headerLines}\r\n\r\n{RequestBody}";
  }

  private static string GetReasonPhrase(HttpStatusCode statusCode) => statusCode switch
  {
    HttpStatusCode.NotFound => "Not Found",
    _ => statusCode.ToString(),
  };
}