using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace codecrafters_http_server.src.Models;

public class HttpResponse(Version httpVersion, HttpHeaders httpHeaders, string? requestBody, byte[]? requestBodyBytes, HttpStatusCode httpStatusCode)
{
  public Version HttpVersion { get; init; } = httpVersion;
  public HttpStatusCode HttpStatusCode { get; init; } = httpStatusCode;
  public HttpHeaders HttpHeaders { get; init; } = httpHeaders;
  public string? RequestBody { get; init; } = requestBody;
  public byte[]? RequestBodyBytes { get; init; } = requestBodyBytes;

  public HttpResponse(Version httpVersion, HttpHeaders httpHeaders, string requestBody, HttpStatusCode httpStatusCode)
    : this(httpVersion, httpHeaders, requestBody, null, httpStatusCode) { }

  public HttpResponse(HttpRequest request, HttpStatusCode httpStatusCode)
    : this(request.HttpVersion, request.HttpHeaders, request.RequestBody, null, httpStatusCode) { }

  public byte[] ToResponseBytes()
  {
    var headerLines = string.Join("\r\n", HttpHeaders.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"));
    var reasonPhrase = GetReasonPhrase(HttpStatusCode);
    var headerString = $"HTTP/{HttpVersion} {(int)HttpStatusCode} {reasonPhrase}\r\n{headerLines}\r\n\r\n";
    var headerBytes = Encoding.ASCII.GetBytes(headerString);

    if (RequestBodyBytes != null)
    {
      var responseBytes = new byte[headerBytes.Length + RequestBodyBytes.Length];
      Buffer.BlockCopy(headerBytes, 0, responseBytes, 0, headerBytes.Length);
      Buffer.BlockCopy(RequestBodyBytes, 0, responseBytes, headerBytes.Length, RequestBodyBytes.Length);
      return responseBytes;
    }

    var bodyBytes = RequestBody != null ? Encoding.UTF8.GetBytes(RequestBody) : Array.Empty<byte>();
    var fullResponseBytes = new byte[headerBytes.Length + bodyBytes.Length];
    Buffer.BlockCopy(headerBytes, 0, fullResponseBytes, 0, headerBytes.Length);
    Buffer.BlockCopy(bodyBytes, 0, fullResponseBytes, headerBytes.Length, bodyBytes.Length);
    return fullResponseBytes;
  }

  private static string GetReasonPhrase(HttpStatusCode statusCode) => statusCode switch
  {
    HttpStatusCode.NotFound => "Not Found",
    _ => statusCode.ToString(),
  };
}
