using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace PLATEAU.Snap.Models.Client;

public class RoofExtractionRequest
{
    [Required]
    [SwaggerSchema("建物面のID", Nullable = false)]
    public int BuildingId { get; set; }

    [Required]
    [SwaggerSchema("面のID", Nullable = false)]
    public int FaceId { get; set; }
}
