using codecrafters_http_server.src.Models;

namespace codecrafters_http_server.src.Handlers;

public interface IHandler
{
  HttpResponse Handle(HttpRequest request, string path);
}
