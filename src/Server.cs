using codecrafters_http_server.src;

const int Port = 4221;

var builder = ServerBuilder.CreateBuilder(args);
var httpServer = builder.Build();
await httpServer.Start(Port);
