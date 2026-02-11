namespace codecrafters_http_server.src.Routing;

public static class RouteConstants
{
  public const string RootPath = "/";
  public const string EchoPathPrefix = "/echo/";
  public const string UserAgentPath = "/user-agent";
  public const string FilesPathPrefix = "/files/";

  public const string RootHandlerKey = "root";
  public const string EchoHandlerKey = "echo";
  public const string UserAgentHandlerKey = "user-agent";
  public const string FileHandlerKey = "file";
}
