using System.Net;
using codecrafters_http_server.src.Helpers;
using codecrafters_http_server.src.Models;

namespace codecrafters_http_server.src.Handlers;

public static class FileHandler
{
  private const string RequestedFilePath = "/files/";
  public static string? FilesDirectory { get; set; }

  public static HttpResponse Handle(HttpRequest request, string path)
  {
    var fileName = path[RequestedFilePath.Length..];
    var fullPath = ResolveFilePath(fileName);

    if (fullPath == null)
      return new HttpResponse(request, HttpStatusCode.NotFound);

    if (request.HttpMethod == HttpMethod.Get)
      return HandleFileGet(request, fullPath);

    if (request.HttpMethod == HttpMethod.Post)
      return HandleFilePost(request, fullPath);

    return new HttpResponse(request, HttpStatusCode.NotFound);
  }

  private static string? ResolveFilePath(string fileName)
  {
    var baseDir = FilesDirectory ?? Directory.GetCurrentDirectory();
    var fullPath = Path.GetFullPath(Path.Combine(baseDir, fileName));
    var baseFull = Path.GetFullPath(baseDir).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

    return fullPath.StartsWith(baseFull, StringComparison.Ordinal) ? fullPath : null;
  }

  private static HttpResponse HandleFileGet(HttpRequest request, string fullPath)
  {
    try
    {
      var fileContent = File.ReadAllText(fullPath);
      var (bodyBytes, contentEncoding) = ResponseHelper.TryCompressIfAccepted(fileContent, request.HttpHeaders);

      const string contentType = "application/octet-stream";
      var headers = bodyBytes != null
        ? ResponseHelper.CreateContentHeaders(bodyBytes, contentType, request.HttpHeaders, contentEncoding)
        : ResponseHelper.CreateContentHeaders(fileContent, contentType, request.HttpHeaders, contentEncoding);

      return bodyBytes != null
        ? new HttpResponse(request.HttpVersion, headers, null, bodyBytes, HttpStatusCode.OK)
        : new HttpResponse(request.HttpVersion, headers, fileContent, HttpStatusCode.OK);
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
