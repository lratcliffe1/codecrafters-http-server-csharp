using System.Net;
using Microsoft.Extensions.DependencyInjection;
using codecrafters_http_server.src.Handlers;
using codecrafters_http_server.src.Models;
using codecrafters_http_server.src.Routing;

namespace codecrafters_http_server.src.Services;

public interface IResponseParser
{
  HttpResponse Parse(HttpRequest request);
}

public class ResponseParser(
  IServiceProvider serviceProvider,
  IRouteMatcher routeMatcher) : IResponseParser
{
  private readonly IServiceProvider _serviceProvider = serviceProvider;
  private readonly IRouteMatcher _routeMatcher = routeMatcher;

  public HttpResponse Parse(HttpRequest request)
  {
    var handlerKey = _routeMatcher.GetHandlerKey(request.HttpTarget);

    if (handlerKey == null)
      return new HttpResponse(request, HttpStatusCode.NotFound);

    var handler = _serviceProvider.GetKeyedService<IHandler>(handlerKey);
    return handler?.Handle(request, request.HttpTarget) ?? new HttpResponse(request, HttpStatusCode.NotFound);
  }
}