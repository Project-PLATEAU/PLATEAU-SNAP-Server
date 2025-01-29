using System.Net;

namespace PLATEAU.Snap.Models.Server;

public class StorageUploadResponse
{
    public HttpStatusCode StatusCode { get; set; }

    public string? Uri { get; set; }

    public StorageUploadResponse()
    { 
    }

    public StorageUploadResponse(HttpStatusCode statusCode) : this(statusCode, null)
    { 
    }

    public StorageUploadResponse(HttpStatusCode statusCode, string? uri)
    {
        StatusCode = statusCode;
        Uri = uri;
    }
}
