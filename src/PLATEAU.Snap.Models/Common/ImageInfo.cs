using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace PLATEAU.Snap.Models.Common;

public class ImageInfo
{
    [Required]
    [SwaggerSchema("画像のID(オルソ画像のときはnull)", ReadOnly = true, Nullable = true)]
    public long? Id { get; set; }

    [Required]
    [SwaggerSchema("建物面のGML ID", ReadOnly = true, Nullable = false)]
    public string Gmlid { get; set; } = null!;

    [Required]
    [SwaggerSchema("サムネイル画像(オルソ画像のときはnull)", ReadOnly = true, Nullable = true)]
    public string? Thumbnail { get; set; } = null!;

    [Required]
    [SwaggerSchema("画像のタイムスタンプ(オルソ画像のときはnull)", ReadOnly = true, Nullable = true)]
    public DateTime? Timestamp { get; set; }

    [Required]
    [SwaggerSchema("オルソ画像かどうか", ReadOnly = true, Nullable = false)]
    public bool IsOrtho { get; set; }

    public ImageInfo()
    {
    }

    public ImageInfo(long? id, string? gmlid, byte[]? thumbnailBytes, DateTime? timestamp, bool isOrtho)
    {
        if (string.IsNullOrEmpty(gmlid))
        {
            throw new ArgumentNullException(nameof(gmlid));
        }

        Id = id;
        Gmlid = gmlid;
        if (thumbnailBytes is not null)
        {
            Thumbnail = $"data:image/jpeg;base64,{Convert.ToBase64String(thumbnailBytes)}";
        }
        Timestamp = timestamp;
        IsOrtho = isOrtho;
    }
}
