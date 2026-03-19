using Swashbuckle.AspNetCore.Annotations;

namespace PLATEAU.Snap.Models.Client;

public class MeshCodeResponse
{
    [SwaggerSchema("3次メッシュコード", Nullable = false)]
    public string MeshCode { get; set; } = null!;
}
