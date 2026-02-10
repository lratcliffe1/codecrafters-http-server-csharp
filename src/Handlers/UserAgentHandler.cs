using System.Net;
using codecrafters_http_server.src.Helpers;
using codecrafters_http_server.src.Models;

namespace codecrafters_http_server.src.Handlers;

public static class UserAgentHandler
{
  public static HttpResponse Handle(HttpRequest request)
  {
    var userAgent = request.HttpHeaders.GetValues("User-Agent").FirstOrDefault() ?? "";
    var headers = ResponseHelper.CreateContentHeaders(userAgent, "text/plain");
    return new HttpResponse(request.HttpVersion, headers, userAgent, HttpStatusCode.OK);
  }
}
