namespace PLATEAU.Snap.Models.Client;

public class Coordinate
{
    public double Longitude { get; set; }

    public double Latitude { get; set; }

    public double Altitude { get; set; }

    public void Validate()
    {
        if (Longitude < -180 || Longitude > 180)
        {
            throw new ArgumentException($"{nameof(Longitude)} must be between -180 and 180.");
        }
        if (Latitude < -90 || Latitude > 90)
        {
            throw new ArgumentException($"{nameof(Latitude)} must be between -90 and 90.");
        }
    }
}
