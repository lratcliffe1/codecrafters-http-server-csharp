using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace codecrafters_http_server.src;

public static class ResponseParser
{
  public static string? FilesDirectory { get; set; }

  private const string RootPath = "/";
  private const string EchoPathPrefix = "/echo/";
  private const string UserAgentPath = "/user-agent";
  private const string RequestedFilePath = "/files/";

  public static HttpResponse Parse(HttpRequest request)
  {
    return request.HttpTarget switch
    {
      RootPath => HandleRoot(request),
      var path when path.StartsWith(EchoPathPrefix, StringComparison.Ordinal) => HandleEcho(request, path),
      var path when path.StartsWith(UserAgentPath, StringComparison.Ordinal) => HandleUserAgent(request),
      var path when path.StartsWith(RequestedFilePath, StringComparison.Ordinal) => HandleFileRequest(request, path),
      _ => new HttpResponse(request, HttpStatusCode.NotFound)
    };
  }

  private static HttpResponse HandleRoot(HttpRequest request)
  {
    return new HttpResponse(request, HttpStatusCode.OK);
  }

  private static HttpResponse HandleEcho(HttpRequest request, string path)
  {
    var body = path[EchoPathPrefix.Length..];
    var headers = CreateContentHeaders(body, "text/plain");
    return new HttpResponse(request.HttpVersion, headers, body, HttpStatusCode.OK);
  }

  private static HttpResponse HandleUserAgent(HttpRequest request)
  {
    var userAgent = request.HttpHeaders.GetValues("User-Agent").FirstOrDefault() ?? "";
    var headers = CreateContentHeaders(userAgent, "text/plain");
    return new HttpResponse(request.HttpVersion, headers, userAgent, HttpStatusCode.OK);
  }

  private static HttpResponse HandleFileRequest(HttpRequest request, string path)
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
      var headers = CreateContentHeaders(fileContent, "application/octet-stream");
      return new HttpResponse(request.HttpVersion, headers, fileContent, HttpStatusCode.OK);
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

  private static HttpContentHeaders CreateContentHeaders(string body, string contentType)
  {
    var headers = new ByteArrayContent([]).Headers;
    headers.Clear();
    headers.Add("Content-Type", contentType);
    headers.Add("Content-Length", Encoding.UTF8.GetByteCount(body).ToString());
    return headers;
  }
}
