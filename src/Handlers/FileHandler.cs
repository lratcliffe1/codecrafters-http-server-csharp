using System.Net;
using codecrafters_http_server.src.Configuration;
using codecrafters_http_server.src.Constants;
using codecrafters_http_server.src.Models;
using codecrafters_http_server.src.Routing;
using codecrafters_http_server.src.Services;

namespace codecrafters_http_server.src.Handlers;

public class FileHandler(IFileConfiguration fileConfiguration, IResponseHeaderBuilder headerBuilder, ICompressionService compressionService) : IHandler
{
  private readonly IFileConfiguration _fileConfiguration = fileConfiguration;
  private readonly IResponseHeaderBuilder _headerBuilder = headerBuilder;
  private readonly ICompressionService _compressionService = compressionService;

  public HttpResponse Handle(HttpRequest request, string path)
  {
    var fileName = path[RouteConstants.FilesPathPrefix.Length..];
    var fullPath = ResolveFilePath(fileName);

    if (fullPath == null)
      return new HttpResponse(request, HttpStatusCode.NotFound);

    if (request.HttpMethod == HttpMethod.Get)
      return HandleFileGet(request, fullPath);

    if (request.HttpMethod == HttpMethod.Post)
      return HandleFilePost(request, fullPath);

    return new HttpResponse(request, HttpStatusCode.NotFound);
  }

  private string? ResolveFilePath(string fileName)
  {
    var baseDir = _fileConfiguration.FilesDirectory;
    var fullPath = Path.GetFullPath(Path.Combine(baseDir, fileName));
    var baseFull = Path.GetFullPath(baseDir).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

    return fullPath.StartsWith(baseFull, StringComparison.Ordinal) ? fullPath : null;
  }

  private HttpResponse HandleFileGet(HttpRequest request, string fullPath)
  {
    try
    {
      var fileContent = File.ReadAllText(fullPath);
      var (bodyBytes, contentEncoding) = _compressionService.TryCompressIfAccepted(fileContent, request.HttpHeaders);

      var headers = bodyBytes != null
        ? _headerBuilder.BuildContentHeaders(bodyBytes, HttpConstants.ContentTypes.ApplicationOctetStream, request.HttpHeaders, contentEncoding)
        : _headerBuilder.BuildContentHeaders(fileContent, HttpConstants.ContentTypes.ApplicationOctetStream, request.HttpHeaders, contentEncoding);

      return bodyBytes != null
        ? new HttpResponse(request.HttpVersion, headers, HttpStatusCode.OK, requestBodyBytes: bodyBytes)
        : new HttpResponse(request.HttpVersion, headers, HttpStatusCode.OK, requestBody: fileContent);
    }
    catch (FileNotFoundException)
    {
      return new HttpResponse(request, HttpStatusCode.NotFound);
    }
  }

  private static HttpResponse HandleFilePost(HttpRequest request, string fullPath)
  {
    try
    {
      File.WriteAllText(fullPath, request.RequestBody);
      return new HttpResponse(request, HttpStatusCode.Created);
    }
    catch (Exception)
    {
      return new HttpResponse(request, HttpStatusCode.InternalServerError);
    }
  }
}
