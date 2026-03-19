using PLATEAU.Snap.Models.Attributes;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace PLATEAU.Snap.Models.Client;

public class ExportRequest
{
    [Required]
    [SwaggerSchema("建物のID", Nullable = false)]
    public int Id { get; set; }

    [ZipFileName]
    [SwaggerSchema("ダウンロードするファイルの名前(拡張子はzip。未指定時は {job_id}.zip となる)", Nullable = true)]
    public string? FileName { get; set; }
}
