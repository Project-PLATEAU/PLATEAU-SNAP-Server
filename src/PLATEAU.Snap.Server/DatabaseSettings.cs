namespace PLATEAU.Snap.Server;

public class DatabaseSettings
{
    /// <summary>
    /// ホスト名またはIPアドレスを取得または設定します。
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// ポート番号を取得または設定します。
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// ユーザー名を取得または設定します。
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// パスワードを取得または設定します。
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// データベース名を取得または設定します。
    /// </summary>
    public string Database { get; set; } = string.Empty;
}
