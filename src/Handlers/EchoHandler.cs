using System.Net;
using codecrafters_http_server.src.Helpers;
using codecrafters_http_server.src.Models;

namespace codecrafters_http_server.src.Handlers;

public static class EchoHandler
{
  private const string EchoPathPrefix = "/echo/";
  private const string ContentType = "text/plain";

  public static HttpResponse Handle(HttpRequest request, string path)
  {
    var body = path[EchoPathPrefix.Length..];
    var (bodyBytes, contentEncoding) = ResponseHelper.TryCompressIfAccepted(body, request.HttpHeaders);

    var headers = bodyBytes != null
      ? ResponseHelper.CreateContentHeaders(bodyBytes, ContentType, contentEncoding)
      : ResponseHelper.CreateContentHeaders(body, ContentType, contentEncoding);

    return bodyBytes != null
      ? new HttpResponse(request.HttpVersion, headers, null, bodyBytes, HttpStatusCode.OK)
      : new HttpResponse(request.HttpVersion, headers, body, HttpStatusCode.OK);
  }
}
