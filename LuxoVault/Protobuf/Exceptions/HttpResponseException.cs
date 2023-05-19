using System.Net;

namespace LuxoVault.Protobuf.Exceptions;

public class HttpResponseException : Exception
{
    public readonly HttpStatusCode StatusCode;

    public HttpResponseException(HttpStatusCode responseStatus) : base(
        $"Failed to load data. Status code: {responseStatus}")
    {
        StatusCode = responseStatus;
    }
}