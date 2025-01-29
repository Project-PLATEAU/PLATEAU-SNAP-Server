using Microsoft.AspNetCore.Http;

namespace PLATEAU.Snap.Models.Server;

public class BuildingImageRequest
{
    public IFormFile File { get; set; } = null!;

    public BuildingImageMetadata Metadata { get; set; } = null!;
}
