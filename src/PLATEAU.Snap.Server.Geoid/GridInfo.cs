namespace PLATEAU.Snap.Server.Geoid;

public class GridInfo
{
    public int MinY { get; }

    public int MinX { get; }

    public float GapY { get; }

    public float GapX { get; }

    public int CountY { get; }

    public int CountX { get; }

    public int Kind { get; }

    public string Version { get; }

    public int DenominatorY { get; } = 60;

    public int DenominatorX { get; } = 40;

    public GridInfo(int minLatitude, int minLongitude, float gapLatitude, float gapLongitude, int latitudeCount, int longitudeCount, int kind, string version)
    {
        this.MinY = minLatitude;
        this.MinX = minLongitude;
        this.GapY = gapLatitude;
        this.GapX = gapLongitude;
        this.CountY = latitudeCount;
        this.CountX = longitudeCount;
        this.Kind = kind;
        this.Version = version;
    }

    public override string ToString()
    {
        return $"MinY: {this.MinY}, MinX: {this.MinX}, GapY: {this.GapY}, GapX: {this.GapX}, CountY: {this.CountY}, CountX: {this.CountX}, Kind: {this.Kind}, Version: {this.Version}";
    }
}
