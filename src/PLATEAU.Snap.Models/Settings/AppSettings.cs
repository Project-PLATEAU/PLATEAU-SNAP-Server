namespace PLATEAU.Snap.Models.Settings;

public class AppSettings
{
    public string ApiKey { get; set; } = null!;

    public string ImportExportToolPath { get; set; } = "/app/3DCityDB-Importer-Exporter/bin/impexp";
}
