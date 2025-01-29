using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace PLATEAU.Snap.Models.Client;

public class VisibleSurfacesRequest
{
    [Required]
    [SwaggerSchema("カメラ座標", Nullable = false)]
    public Coordinate From { get; set; } = null!;

    [Required]
    [SwaggerSchema("カメラを向けている座標", Nullable = false)]
    public Coordinate To { get; set; } = null!;

    [Required]
    [SwaggerSchema("カメラのロール角 (度数法で半時計回りを正とする)", Nullable = false)]
    public double Roll { get; set; }

    [SwaggerSchema("検出する最大距離 (メートル)", Nullable = true)]
    public double? MaxDistance { get; set; }

    [SwaggerSchema("カメラの視野角 (度数法)", Nullable = true)]
    public double? FieldOfView { get; set; }

    public Server.VisibleSurfacesRequest ToServerParam()
    {
        return new Server.VisibleSurfacesRequest
        {
            From = new NetTopologySuite.Geometries.CoordinateZ(From.Longitude, From.Latitude, From.Altitude),
            To = new NetTopologySuite.Geometries.CoordinateZ(To.Longitude, To.Latitude, To.Altitude),
            Roll = Roll,
            MinDistance = 2,
            MaxDistance = MaxDistance.HasValue ? MaxDistance.Value : 100,
            FieldOfView = FieldOfView.HasValue ? FieldOfView.Value : 45
        };
    }
}
