namespace PLATEAU.Snap.Models.Lambda;

public class LambdaRoofExtractionRequest
{
    /// <summary>
    /// 屋根面のジオメトリ (WKT形式)
    /// </summary>
    public string Geometry { get; set; } = null!;
}
