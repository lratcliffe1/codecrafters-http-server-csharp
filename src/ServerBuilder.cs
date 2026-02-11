using Microsoft.Extensions.DependencyInjection;
using codecrafters_http_server.src.Configuration;
using codecrafters_http_server.src.Handlers;
using codecrafters_http_server.src.Routing;
using codecrafters_http_server.src.Services;

namespace codecrafters_http_server.src;

public class ServerBuilder
{
  public IServiceCollection Services { get; }

  private ServerBuilder()
  {
    Services = new ServiceCollection();
  }

  public static ServerBuilder CreateBuilder(string[] args)
  {
    var builder = new ServerBuilder();
    var filesDirectory = ParseFilesDirectory(args);

    // Register configuration (singleton)
    builder.Services.AddSingleton<IFileConfiguration>(sp => new FileConfiguration(filesDirectory ?? Directory.GetCurrentDirectory()));

    // Register routing (singleton)
    builder.Services.AddSingleton<IRouteMatcher, RouteMatcher>();

    // Register shared services (singleton)
    builder.Services.AddSingleton<IResponseHeaderBuilder, ResponseHeaderBuilder>();
    builder.Services.AddSingleton<ICompressionService, CompressionService>();

    // Register handlers with keys (scoped - one per client connection)
    builder.Services.AddKeyedScoped<IHandler, RootHandler>(RouteConstants.RootHandlerKey);
    builder.Services.AddKeyedScoped<IHandler, EchoHandler>(RouteConstants.EchoHandlerKey);
    builder.Services.AddKeyedScoped<IHandler, UserAgentHandler>(RouteConstants.UserAgentHandlerKey);
    builder.Services.AddKeyedScoped<IHandler, FileHandler>(RouteConstants.FileHandlerKey);

    // Register scoped services (one per client connection)
    builder.Services.AddScoped<IRequestReader, RequestReader>();
    builder.Services.AddScoped<IResponseParser, ResponseParser>();
    builder.Services.AddScoped<IClientConnectionHandler, ClientConnectionHandler>();

    return builder;
  }

  private static string? ParseFilesDirectory(string[] args)
  {
    for (var i = 0; i < args.Length - 1; i++)
    {
      if (args[i] == "--directory")
        return args[++i];
    }
    return null;
  }

  public HttpServer Build()
  {
    var serviceProvider = Services.BuildServiceProvider();
    return new HttpServer(serviceProvider);
  }
}
