using System;
using System.Collections.Generic;

namespace PLATEAU.Snap.Server.Entities.Models;

public partial class Job
{
    public long Id { get; set; }

    public string Type { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? Parameter { get; set; }

    public string? ResultParameter { get; set; }

    public string? Message { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
