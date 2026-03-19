using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PLATEAU.Snap.Models.Common;

public class BuildingImage
{
    [Required]
    [SwaggerSchema("建物のID", ReadOnly = true, Nullable = false)]
    public int Id { get; set; }

    [Required]
    [SwaggerSchema("建物面のGML ID", ReadOnly = true, Nullable = false)]
    public string Gmlid { get; set; } = null!;

    [Required]
    [SwaggerSchema("サムネイル画像", ReadOnly = true, Nullable = false)]
    [NotMapped]
    public string? Thumbnail => ThumbnailBytes != null ? $"data:image/jpeg;base64,{Convert.ToBase64String(ThumbnailBytes)}" : null;

    [Required]
    [SwaggerSchema("住所", ReadOnly = true, Nullable = false)]
    public string Address { get; set; } = null!;

    [JsonIgnore]
    public byte[] ThumbnailBytes { get; set; } = null!;

    public BuildingImage()
    {
    }
}
