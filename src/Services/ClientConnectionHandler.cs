using System.Net;
using System.Net.Sockets;
using System.Text;
using codecrafters_http_server.src.Constants;
using codecrafters_http_server.src.Models;

namespace codecrafters_http_server.src.Services;

public interface IClientConnectionHandler
{
  void HandleClient(TcpClient client);
}

public class ClientConnectionHandler(IResponseParser responseParser, IRequestReader requestReader) : IClientConnectionHandler
{
  private readonly IResponseParser _responseParser = responseParser;
  private readonly IRequestReader _requestReader = requestReader;

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
          SendErrorResponse(stream, ex);
          break;
        }
      }
    }
    catch (Exception ex)
    {
      Console.Error.WriteLine($"Error handling client: {ex.Message}");
    }
  }

  private static void SendErrorResponse(NetworkStream stream, Exception ex)
  {
    Console.Error.WriteLine($"Error processing request: {ex.Message}");
    try
    {
      var responseMessage = new HttpResponseMessage();
      var headers = responseMessage.Headers;
      var errorResponse = new HttpResponse(new Version(1, 1), headers, HttpStatusCode.BadRequest, HttpConstants.ErrorMessages.BadRequestMessage);
      var errorBytes = errorResponse.ToResponseBytes();
      stream.Write(errorBytes, 0, errorBytes.Length);
      stream.Flush();
    }
    catch (Exception writeEx)
    {
      Console.Error.WriteLine($"Failed to send error response: {writeEx.Message}");
    }
  }
}
