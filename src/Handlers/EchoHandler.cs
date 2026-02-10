using System.Net;
using codecrafters_http_server.src.Helpers;
using codecrafters_http_server.src.Models;

namespace codecrafters_http_server.src.Handlers;

public static class EchoHandler
{
  private const string EchoPathPrefix = "/echo/";

  public static HttpResponse Handle(HttpRequest request, string path)
  {
    var body = path[EchoPathPrefix.Length..];
    string? contentEncoding = null;
    byte[]? bodyBytes = null;

    if (request.HttpHeaders.TryGetValues("Accept-Encoding", out var acceptEncoding))
    {
      if (acceptEncoding.Contains("gzip"))
      {
        bodyBytes = ResponseHelper.GzipCompress(body);
        contentEncoding = "gzip";
      }
    }

    var headers = bodyBytes != null
      ? ResponseHelper.CreateContentHeaders(bodyBytes, "text/plain", contentEncoding)
      : ResponseHelper.CreateContentHeaders(body, "text/plain", contentEncoding);

    return bodyBytes != null
      ? new HttpResponse(request.HttpVersion, headers, null, bodyBytes, HttpStatusCode.OK)
      : new HttpResponse(request.HttpVersion, headers, body, HttpStatusCode.OK);
  }
}
