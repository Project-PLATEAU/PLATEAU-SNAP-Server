namespace PLATEAU.Snap.Models.Lambda;

public class LambdaApplyTextureResponse : LambdaResponseBody
{
    /// <summary>
    /// 画像のパス
    /// </summary>
    public string Path { get; set; } = null!;

    /// <summary>
    /// 正規化済テクスチャ座標 (WKT形式)
    /// </summary>
    public string TextureCoordinates { get; set; } = null!;
}
