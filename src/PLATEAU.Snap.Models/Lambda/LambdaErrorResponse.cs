using System.Text.Json.Serialization;

namespace PLATEAU.Snap.Models.Lambda;

public class LambdaErrorResponse
{
    /// <summary>
    /// エラーメッセージ
    /// </summary>
    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; } = null!;
}
