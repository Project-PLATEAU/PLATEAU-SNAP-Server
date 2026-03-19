using System;
using System.Collections.Generic;

namespace PLATEAU.Snap.Server.Entities.Models;

public partial class TexImage
{
    public int Id { get; set; }

    public string? TexImageUri { get; set; }

    public byte[]? TexImageData { get; set; }

    public string? TexMimeType { get; set; }

    public string? TexMimeTypeCodespace { get; set; }

    public virtual ICollection<SurfaceDatum> SurfaceData { get; set; } = new List<SurfaceDatum>();
}
