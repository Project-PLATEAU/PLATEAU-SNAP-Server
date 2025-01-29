using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace PLATEAU.Snap.Models.Client;

public class BuildingImageRequest
{
    [Required]
    [SwaggerSchema("建物面を撮影した画像ファイル", Nullable = false)]
    public IFormFile File { get; set; } = null!;

    [Required]
    [SwaggerSchema("画像に関連するメタデータ", Nullable = false)]
    public string Metadata { get; set; } = null!;

    public Server.BuildingImageRequest ToServerParam()
    {
        ArgumentNullException.ThrowIfNull(File, nameof(File));
        ArgumentNullException.ThrowIfNull(Metadata, nameof(Metadata));

        try
        {
            var metadata = Util.Deserialize<Server.BuildingImageMetadata>(Metadata);
            if (metadata == null)
            {
                throw new InvalidCastException("Failed to deserialize metadata");
            }
            metadata.Validate();

            return new Server.BuildingImageRequest
            {
                File = File,
                Metadata = metadata
            };
        }
        catch (JsonException)
        {
            throw new InvalidCastException("Failed to deserialize metadata");
        }
    }
}
