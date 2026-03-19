using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace PLATEAU.Snap.Models.Client;

public class PreviewTextureResponse
{
    [Required]
    [SwaggerSchema("glTF Embedded の base64 エンコード", ReadOnly = true, Nullable = false)]
    public string Gltf { get; set; } = null!;
}
