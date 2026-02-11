using System.Net;
using codecrafters_http_server.src.Models;

namespace codecrafters_http_server.src.Handlers;

public class RootHandler : IHandler
{
  public HttpResponse Handle(HttpRequest request, string path)
  {
    return new HttpResponse(request, HttpStatusCode.OK);
  }
}
