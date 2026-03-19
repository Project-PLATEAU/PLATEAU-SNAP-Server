using PLATEAU.Snap.Models.Attributes;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace PLATEAU.Snap.Models.Client;

public class ExportMeshRequest
{
    [Required]
    [SwaggerSchema("3次メッシュコード", Nullable = false)]
    public string MeshCode { get; set; } = null!;

    [ZipFileName]
    [SwaggerSchema("ダウンロードするファイルの名前(拡張子はzip。未指定時は {job_id}.zip となる)", Nullable = true)]
    public string? FileName { get; set; }
}
