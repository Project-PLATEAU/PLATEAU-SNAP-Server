namespace PLATEAU.Snap.Models.Lambda;

public class LambdaApplyTextureRequest
{
    /// <summary>
    /// 画像のパス
    /// </summary>
    public string Path { get; set; } = null!;

    /// <summary>
    /// 画像内の建物座標 (WKT形式)
    /// </summary>
    public string Coordinates { get; set; } = null!;
}
