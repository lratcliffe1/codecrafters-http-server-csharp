using System.Net;
using codecrafters_http_server.src.Constants;
using codecrafters_http_server.src.Models;
using codecrafters_http_server.src.Services;

namespace codecrafters_http_server.src.Handlers;

public class UserAgentHandler(IResponseHeaderBuilder headerBuilder) : IHandler
{
  private readonly IResponseHeaderBuilder _headerBuilder = headerBuilder;

  public HttpResponse Handle(HttpRequest request, string path)
  {
    var userAgent = request.HttpHeaders.GetValues(HttpConstants.HttpHeaderNames.UserAgent).FirstOrDefault() ?? "";
    var headers = _headerBuilder.BuildContentHeaders(userAgent, HttpConstants.ContentTypes.TextPlain, request.HttpHeaders);
    return new HttpResponse(request.HttpVersion, headers, HttpStatusCode.OK, userAgent);
  }
}
