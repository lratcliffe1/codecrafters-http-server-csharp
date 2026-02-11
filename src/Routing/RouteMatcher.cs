namespace codecrafters_http_server.src.Routing;

public interface IRouteMatcher
{
  string? GetHandlerKey(string path);
}

public class RouteMatcher : IRouteMatcher
{
  public string? GetHandlerKey(string path)
  {
    return path switch
    {
      RouteConstants.RootPath => RouteConstants.RootHandlerKey,
      var p when p.StartsWith(RouteConstants.EchoPathPrefix, StringComparison.Ordinal) => RouteConstants.EchoHandlerKey,
      var p when p.StartsWith(RouteConstants.UserAgentPath, StringComparison.Ordinal) => RouteConstants.UserAgentHandlerKey,
      var p when p.StartsWith(RouteConstants.FilesPathPrefix, StringComparison.Ordinal) => RouteConstants.FileHandlerKey,
      _ => null
    };
  }
}
