using System;
using System.Collections.Generic;

namespace PLATEAU.Snap.Server.Entities.Models;

public partial class ImageSurfaceRelation
{
    public long Id { get; set; }

    public long ImageId { get; set; }

    public string Gmlid { get; set; } = null!;

    public virtual Image Image { get; set; } = null!;
}
