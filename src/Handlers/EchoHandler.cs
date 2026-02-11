using System.Net;
using codecrafters_http_server.src.Constants;
using codecrafters_http_server.src.Models;
using codecrafters_http_server.src.Routing;
using codecrafters_http_server.src.Services;

namespace codecrafters_http_server.src.Handlers;

public class EchoHandler(IResponseHeaderBuilder headerBuilder, ICompressionService compressionService) : IHandler
{
  private readonly IResponseHeaderBuilder _headerBuilder = headerBuilder;
  private readonly ICompressionService _compressionService = compressionService;

  public HttpResponse Handle(HttpRequest request, string path)
  {
    var body = path[RouteConstants.EchoPathPrefix.Length..];
    var (bodyBytes, contentEncoding) = _compressionService.TryCompressIfAccepted(body, request.HttpHeaders);

    var headers = bodyBytes != null
      ? _headerBuilder.BuildContentHeaders(bodyBytes, HttpConstants.ContentTypes.TextPlain, request.HttpHeaders, contentEncoding)
      : _headerBuilder.BuildContentHeaders(body, HttpConstants.ContentTypes.TextPlain, request.HttpHeaders, contentEncoding);

    return bodyBytes != null
      ? new HttpResponse(request.HttpVersion, headers, HttpStatusCode.OK, requestBodyBytes: bodyBytes)
      : new HttpResponse(request.HttpVersion, headers, HttpStatusCode.OK, requestBody: body);
  }
}
