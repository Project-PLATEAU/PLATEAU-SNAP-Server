using PLATEAU.Snap.Models.Common;

namespace PLATEAU.Snap.Models.Client;

public class Job
{
    public long Id { get; set; }

    public JobType Type { get; set; }

    public JobStatusType Status { get; set; }

    public AbstractJobParam? Parameter { get; set; }

    public AbstractJobResultParam? ResultParameter { get; set; }

    public string? Message { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
