using System.Net;
using System.Text;

namespace codecrafters_http_server.src;

public static class ResponseParser
{
  public static HttpResponse Parse(HttpRequest request)
  {
    if (request.HttpTarget == "/")
      return new HttpResponse(request, HttpStatusCode.OK);

    if (request.HttpTarget.StartsWith("/echo/"))
    {
      var body = request.HttpTarget.Split("/echo/").Last();
      var headers = new ByteArrayContent([]).Headers;
      headers.Clear();
      headers.Add("Content-Type", "text/plain");
      headers.Add("Content-Length", Encoding.UTF8.GetByteCount(body).ToString());
      return new HttpResponse(request.HttpVersion, headers, body, HttpStatusCode.OK);
    }

    return new HttpResponse(request, HttpStatusCode.NotFound);
  }
}
