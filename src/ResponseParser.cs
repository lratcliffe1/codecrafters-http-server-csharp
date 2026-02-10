using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace codecrafters_http_server.src;

public static class ResponseParser
{
  private const string RootPath = "/";
  private const string EchoPathPrefix = "/echo/";
  private const string UserAgentPath = "/user-agent";

  public static HttpResponse Parse(HttpRequest request)
  {
    if (request.HttpTarget == RootPath)
      return new HttpResponse(request, HttpStatusCode.OK);

    if (request.HttpTarget.StartsWith(EchoPathPrefix, StringComparison.Ordinal))
    {
      var body = request.HttpTarget[EchoPathPrefix.Length..];
      return new HttpResponse(request.HttpVersion, CreateTextPlainHeaders(body), body, HttpStatusCode.OK);
    }

    if (request.HttpTarget.StartsWith(UserAgentPath, StringComparison.Ordinal))
    {
      var userAgent = request.HttpHeaders.GetValues("User-Agent").FirstOrDefault() ?? "";
      return new HttpResponse(request.HttpVersion, CreateTextPlainHeaders(userAgent), userAgent, HttpStatusCode.OK);
    }

    return new HttpResponse(request, HttpStatusCode.NotFound);
  }

  private static HttpHeaders CreateTextPlainHeaders(string body)
  {
    var headers = new ByteArrayContent([]).Headers;
    headers.Clear();
    headers.Add("Content-Type", "text/plain");
    headers.Add("Content-Length", Encoding.UTF8.GetByteCount(body).ToString());
    return headers;
  }
}
