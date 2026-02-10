using System.Net.Http.Headers;

namespace codecrafters_http_server.src;

public class HttpRequest(HttpMethod httpMethod, string httpTarget, Version httpVersion, HttpHeaders httpHeaders, string requestBody)
{
  public HttpMethod HttpMethod { get; init; } = httpMethod;
  public string HttpTarget { get; init; } = httpTarget;
  public Version HttpVersion { get; init; } = httpVersion;
  public HttpHeaders HttpHeaders { get; init; } = httpHeaders;
  public string RequestBody { get; init; } = requestBody;
}