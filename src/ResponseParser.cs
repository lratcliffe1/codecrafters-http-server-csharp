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
    if (request.HttpTarget == RootPath)
      return new HttpResponse(request, HttpStatusCode.OK);

    if (request.HttpTarget.StartsWith(EchoPathPrefix, StringComparison.Ordinal))
    {
      var body = request.HttpTarget[EchoPathPrefix.Length..];
      return new HttpResponse(request.HttpVersion, CreateTextPlainHeaders(body, "text/plain"), body, HttpStatusCode.OK);
    }

    if (request.HttpTarget.StartsWith(UserAgentPath, StringComparison.Ordinal))
    {
      var userAgent = request.HttpHeaders.GetValues("User-Agent").FirstOrDefault() ?? "";
      return new HttpResponse(request.HttpVersion, CreateTextPlainHeaders(userAgent, "text/plain"), userAgent, HttpStatusCode.OK);
    }

    if (request.HttpTarget.StartsWith(RequestedFilePath, StringComparison.Ordinal))
    {
      var fileName = request.HttpTarget[RequestedFilePath.Length..];
      var baseDir = FilesDirectory ?? Directory.GetCurrentDirectory();
      var fullPath = Path.GetFullPath(Path.Combine(baseDir, fileName));
      var baseFull = Path.GetFullPath(baseDir).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
      if (!fullPath.StartsWith(baseFull, StringComparison.Ordinal))
      {
        return new HttpResponse(request, HttpStatusCode.NotFound);
      }

      try
      {
        var fileContent = File.ReadAllText(fullPath);
        return new HttpResponse(request.HttpVersion, CreateTextPlainHeaders(fileContent, "application/octet-stream"), fileContent, HttpStatusCode.OK);
      }
      catch (FileNotFoundException)
      {
        return new HttpResponse(request, HttpStatusCode.NotFound);
      }
    }

    return new HttpResponse(request, HttpStatusCode.NotFound);
  }

  private static HttpContentHeaders CreateTextPlainHeaders(string body, string contentType)
  {
    var headers = new ByteArrayContent([]).Headers;
    headers.Clear();
    headers.Add("Content-Type", contentType);
    headers.Add("Content-Length", Encoding.UTF8.GetByteCount(body).ToString());
    return headers;
  }
}
