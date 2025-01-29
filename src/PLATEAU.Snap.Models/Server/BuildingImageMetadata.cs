using PLATEAU.Snap.Models.Client;

namespace PLATEAU.Snap.Models.Server;

public class BuildingImageMetadata
{
    public string Gmlid { get; set; } = null!;

    public Coordinate From { get; set; } = null!;

    public Coordinate To { get; set; } = null!;

    public double Roll { get; set; }

    public DateTime Timestamp { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Gmlid))
        {
            throw new ArgumentException($"{nameof(Gmlid)} is required.");
        }
        if (From == null)
        {
            throw new ArgumentException($"{nameof(From)} is required.");
        }
        if (To == null)
        {
            throw new ArgumentException($"{nameof(To)} is required.");
        }
        if (Timestamp == default)
        {
            throw new ArgumentException($"{nameof(Timestamp)} is required.");
        }

        From.Validate();
        To.Validate();
    }
}
