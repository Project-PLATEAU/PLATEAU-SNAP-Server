namespace PLATEAU.Snap.Models.Client;

public class BuildingImageResponse
{
    public StatusType Status { get; set; }

    public long? Id { get; set; }

    public string? Message { get; set; }

    public SnapServerException? Exception { get; set; }
}
