using System.Net;
using codecrafters_http_server.src.Constants;
using codecrafters_http_server.src.Models;

namespace codecrafters_http_server.src.Services;

public interface IErrorResponseFactory
{
  HttpResponse CreateBadRequestResponse();
  byte[] CreateBadRequestResponseBytes();
}

public class ErrorResponseFactory : IErrorResponseFactory
{
  public HttpResponse CreateBadRequestResponse()
  {
    var responseMessage = new HttpResponseMessage();
    var headers = responseMessage.Headers;
    return new HttpResponse(new Version(1, 1), headers, HttpStatusCode.BadRequest, HttpConstants.ErrorMessages.BadRequestMessage);
  }

  public byte[] CreateBadRequestResponseBytes()
  {
    return CreateBadRequestResponse().ToResponseBytes();
  }
}
