using System.Net;
using codecrafters_http_server.src.Handlers;
using codecrafters_http_server.src.Models;

namespace codecrafters_http_server.src;

public static class ResponseParser
{
  public static string? FilesDirectory
  {
    get => FileHandler.FilesDirectory;
    set => FileHandler.FilesDirectory = value;
  }

  private const string RootPath = "/";
  private const string EchoPathPrefix = "/echo/";
  private const string UserAgentPath = "/user-agent";
  private const string FilesPathPrefix = "/files/";

  public static HttpResponse Parse(HttpRequest request)
  {
    return request.HttpTarget switch
    {
      RootPath => RootHandler.Handle(request),
      var path when path.StartsWith(EchoPathPrefix, StringComparison.Ordinal) => EchoHandler.Handle(request, path),
      var path when path.StartsWith(UserAgentPath, StringComparison.Ordinal) => UserAgentHandler.Handle(request),
      var path when path.StartsWith(FilesPathPrefix, StringComparison.Ordinal) => FileHandler.Handle(request, path),
      _ => new HttpResponse(request, HttpStatusCode.NotFound)
    };
  }
}