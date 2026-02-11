using System.Net.Sockets;
using System.Text;

namespace codecrafters_http_server.src.Services;

public interface IClientConnectionHandler
{
  void HandleClient(TcpClient client);
}

public class ClientConnectionHandler(IResponseParser responseParser, IRequestReader requestReader, IErrorResponseFactory errorResponseFactory) : IClientConnectionHandler
{
  private readonly IResponseParser _responseParser = responseParser;
  private readonly IRequestReader _requestReader = requestReader;
  private readonly IErrorResponseFactory _errorResponseFactory = errorResponseFactory;

  public void HandleClient(TcpClient client)
  {
    try
    {
      using var _ = client;
      using var stream = client.GetStream();
      var requestBuilder = new StringBuilder();

      while (true)
      {
        try
        {
          // Process all complete requests in the buffer
          while (true)
          {
            var request = _requestReader.TryParseCompleteRequest(requestBuilder);
            if (request == null)
              break; // No complete request yet

            var responseBytes = _responseParser.Parse(request).ToResponseBytes();
            stream.Write(responseBytes, 0, responseBytes.Length);
            stream.Flush();

            if (_requestReader.ShouldCloseConnection(request))
              return;
          }

          // Read more data from stream
          if (!_requestReader.TryReadFromStream(stream, requestBuilder))
            break; // Stream closed
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine($"Error processing request: {ex.Message}");
          try
          {
            var errorBytes = _errorResponseFactory.CreateBadRequestResponseBytes();
            stream.Write(errorBytes, 0, errorBytes.Length);
            stream.Flush();
          }
          catch (Exception writeEx)
          {
            Console.Error.WriteLine($"Failed to send error response: {writeEx.Message}");
          }
          break;
        }
      }
    }
    catch (Exception ex)
    {
      Console.Error.WriteLine($"Error handling client: {ex.Message}");
    }
  }
}
