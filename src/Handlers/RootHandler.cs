using System.Net;
using codecrafters_http_server.src.Models;

namespace codecrafters_http_server.src.Handlers;

public static class RootHandler
{
  public static HttpResponse Handle(HttpRequest request)
  {
    return new HttpResponse(request, HttpStatusCode.OK);
  }
}
