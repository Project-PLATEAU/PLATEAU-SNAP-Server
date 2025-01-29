using PLATEAU.Snap.Models.Server;

namespace PLATEAU.Snap.Server.Entities.Models;

public partial class Image
{
    public Image()
    {
    }

    public Image(BuildingImageMetadata metadata)
    {
        FromLatitude = metadata.From.Latitude;
        FromLongitude = metadata.From.Longitude;
        FromAltitude = metadata.From.Altitude;
        ToLatitude = metadata.To.Latitude;
        ToLongitude = metadata.To.Longitude;
        ToAltitude = metadata.To.Altitude;
        Roll = metadata.Roll;
        Timestamp = metadata.Timestamp;
        ImageSurfaceRelations.Add(new ImageSurfaceRelation { Gmlid = metadata.Gmlid });
    }
}
