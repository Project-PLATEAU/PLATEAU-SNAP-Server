using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PLATEAU.Snap.Models.Client;

public class ApplyTextureRequest
{
    private NetTopologySuite.Geometries.Polygon? polygon;

    [Required]
    [SwaggerSchema("建物面のID", Nullable = false)]
    public int BuildingId { get; set; }

    [Required]
    [SwaggerSchema("面のID", Nullable = false)]
    public int FaceId { get; set; }

    [Required]
    [SwaggerSchema("transform または roof-extraction で取得した画像のパス", Nullable = false)]
    public string Path { get; set; } = null!;

    [Required]
    [SwaggerSchema("座標情報 (WKT形式)", Nullable = false)]
    public string Coordinates { get; set; } = null!;

    [JsonIgnore]
    public NetTopologySuite.Geometries.Polygon Polygon
    {
        get
        {
            if (polygon is null)
            {
                try
                {
                    var reader = new NetTopologySuite.IO.WKTReader();
                    var temp = reader.Read(Coordinates) as NetTopologySuite.Geometries.Polygon;
                    if (temp is null || !temp.IsValid)
                    {
                        throw new ArgumentException($"{nameof(Coordinates)} is not a valid polygon.");
                    }
                    polygon = temp;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Failed to parse {nameof(Coordinates)}: {ex.Message}", ex);
                }
            }
            return polygon;
        }
    }
}
