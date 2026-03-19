using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace PLATEAU.Snap.Models.Client;

public class TransformResponse
{
    [Required]
    [SwaggerSchema("画像のパス", ReadOnly = true, Nullable = true)]
    public string? Path { get; set; }

    [Required]
    [SwaggerSchema("画像をダウンロードするための Presigned URL", ReadOnly = true, Nullable = true)]
    public string? Uri { get; set; }

    [Required]
    [SwaggerSchema("座標情報 (WKT形式)", ReadOnly = true, Nullable = true)]
    public string? Coordinates { get; set; }

    public TransformResponse()
    {
    }

    public TransformResponse(string? path, string? uri, string? coordinates)
    {
        Path = path;
        Uri = uri;
        Coordinates = coordinates;
    }
}
