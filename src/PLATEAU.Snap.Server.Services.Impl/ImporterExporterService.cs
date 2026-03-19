using NetTopologySuite.Geometries;
using PLATEAU.Snap.Models;
using PLATEAU.Snap.Models.Common;
using PLATEAU.Snap.Models.Exceptions;
using PLATEAU.Snap.Models.Settings;
using PLATEAU.Snap.Server.Repositories;
using System.Diagnostics;
using System.IO.Compression;
using System.Xml;

namespace PLATEAU.Snap.Server.Services;

internal class ImporterExporterService : IImporterExporterService
{
    private const string CityGmlSchemaLocation = "https://www.geospatial.jp/iur/uro/3.0 ../../schemas/iur/uro/3.0/urbanObject.xsd http://www.opengis.net/citygml/2.0 http://schemas.opengis.net/citygml/2.0/cityGMLBase.xsd http://www.opengis.net/citygml/landuse/2.0 http://schemas.opengis.net/citygml/landuse/2.0/landUse.xsd http://www.opengis.net/citygml/building/2.0 http://schemas.opengis.net/citygml/building/2.0/building.xsd http://www.opengis.net/citygml/transportation/2.0 http://schemas.opengis.net/citygml/transportation/2.0/transportation.xsd http://www.opengis.net/citygml/generics/2.0 http://schemas.opengis.net/citygml/generics/2.0/generics.xsd http://www.opengis.net/citygml/relief/2.0 http://schemas.opengis.net/citygml/relief/2.0/relief.xsd http://www.opengis.net/citygml/cityobjectgroup/2.0 http://schemas.opengis.net/citygml/cityobjectgroup/2.0/cityObjectGroup.xsd http://www.opengis.net/gml http://schemas.opengis.net/gml/3.1.1/base/gml.xsd http://www.opengis.net/citygml/appearance/2.0 http://schemas.opengis.net/citygml/appearance/2.0/appearance.xsd";

    private const string NamespaceCityGml2_0 = "http://www.opengis.net/citygml/2.0";

    private readonly ISurfaceGeometryRepository surfaceGeometryRepository;

    private readonly IStorageRepository storageRepository;

    private readonly IJobRepository jobRepository;

    private readonly AppSettings appSettings;

    private readonly DatabaseSettings databaseSettings;

    public ImporterExporterService(ISurfaceGeometryRepository surfaceGeometryRepository, IStorageRepository storageRepository, IJobRepository jobRepository, AppSettings appSettings, DatabaseSettings databaseSettings)
    {
        this.surfaceGeometryRepository = surfaceGeometryRepository;
        this.storageRepository = storageRepository;
        this.jobRepository = jobRepository;
        this.appSettings = appSettings;
        this.databaseSettings = databaseSettings;
    }

    public async Task<ExportBuildingResultParam> ExportAsync(Models.Lambda.LambdaExportBuildingRequest request)
    {
        return await ExecuteJob(request.JobId, async (ExportBuildingParam param) =>
        {
            var stream = await ExportAsync(request.BuildingId);
            var fileName = param.FileName ?? $"{request.JobId}.zip";
            var path = $"temp/export/{Guid.NewGuid()}/{fileName}";

            var uploadResult = await this.storageRepository.UploadAsync(stream, path);
            return new ExportBuildingResultParam
            {
                Path = uploadResult.Uri!,
            };
        });
    }

    public async Task<ExportMeshResultParam> ExportAsync(Models.Lambda.LambdaExportMeshRequest request)
    {
        return await ExecuteJob(request.JobId, async (ExportMeshParam param) =>
        {
            var stream = await ExportAsync(request.MeshCode);
            var fileName = param.FileName ?? $"{request.JobId}.zip";
            var path = $"temp/export/{Guid.NewGuid()}/{fileName}";

            var uploadResult = await this.storageRepository.UploadAsync(stream, path);
            return new ExportMeshResultParam
            {
                Path = uploadResult.Uri!,
            };
        });
    }

    public async Task<Stream> ExportAsync(int buildingId)
    {
        // メッシュコードの取得
        var roofprint = await this.surfaceGeometryRepository.GetRoofprintAsync(buildingId);
        if (roofprint is null)
        {
            throw new NotFoundException($"Building with ID {buildingId} does not exist.");
        }
        var meshCode = MeshUtil.GetThirdMeshCode(roofprint);

        // 建物のEnvelopeを取得
        var envelope = await this.surfaceGeometryRepository.GetEnvelopeGeometryAsync(buildingId);
        if (envelope is null)
        {
            throw new InvalidOperationException($"Building with ID {buildingId} does not exist.");
        }

        return await ExportAsync([buildingId], meshCode, envelope);
    }

    public async Task<Stream> ExportAsync(string meshCode)
    {
        // メッシュコードのEnvelopeを取得
        var envelope = MeshUtil.GetEnvelopeFromThirdMeshCode(meshCode);
        var geometryFactory = new GeometryFactory();
        var polygon = geometryFactory.ToGeometry(envelope) as Polygon;

        // メッシュに重なる建物IDを取得
        var intersections = await this.surfaceGeometryRepository.GetIntersectionsAsync(polygon!);

        // メッシュの境界上の建物について、このメッシュに含めるべきか判定
        var notContains = await this.surfaceGeometryRepository.GetNotContainsAsync(polygon!, intersections.ToArray());
        var targetSet = new HashSet<int>(intersections);
        foreach (var entry in notContains)
        {
            var meshCodeFromGeom = MeshUtil.GetThirdMeshCode(entry.Geom);
            if (meshCodeFromGeom != meshCode)
            {
                targetSet.Remove(entry.Id);
            }
        }

        return await ExportAsync(targetSet, meshCode, polygon!);
    }

    private async Task<TResult> ExecuteJob<TParam, TResult>(long jobId, Func<TParam, Task<TResult>> func)
    {
        var job = await this.jobRepository.GetAsync(jobId);
        if (job == null)
        {
            throw new NotFoundException($"Job with ID {jobId} does not exist.");
        }

        try
        {
            var param = Util.Deserialize<TParam>(job.Parameter!);
            if (param == null)
            {
                throw new InvalidOperationException("Job parameter is invalid.");
            }

            job.Status = JobStatusType.in_progress.ToString();
            await this.jobRepository.UpdateAsync(job);

            var resultParam = await func(param);

            job.Status = JobStatusType.completed.ToString();
            job.Message = "正常に終了しました。";
            job.ResultParameter = Util.Serialize(resultParam);
            await this.jobRepository.UpdateAsync(job);

            return resultParam;
        }
        catch (Exception ex)
        {
            job.Status = JobStatusType.failed.ToString();
            job.Message = $"異常が検出されたため、処理を中断しました。詳細: {ex.Message}";
            await this.jobRepository.UpdateAsync(job);
            throw;
        }
    }

    private async Task<Stream> ExportAsync(IEnumerable<int> idList, string meshCode, Geometry envelope)
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            // 出力内容を補正するためにツールではファイルとして出力し、自前でzip化している
            // 仮に補正が不要だとしても、3D City Database Importer/Exporter に不具合があり、zip出力するとzipを閉じる際に例外がスローされることがあるため注意
            var outputDirectory = Path.Combine(tempDirectory, "bldg");
            var filePath = Path.Combine(outputDirectory, $"{meshCode}.gml");
            Directory.CreateDirectory(outputDirectory!);
            var arguments = $"export -H {databaseSettings.Host} -P {databaseSettings.Port} -u {databaseSettings.Username} -p {databaseSettings.Password} -d {databaseSettings.Database} --db-id {string.Join(",", idList)} -o {filePath}";

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = appSettings.ImportExportToolPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"Export failed with exit code {process.ExitCode}. Output: {output}, Error: {error}");
            }

            // PLATEAU の CityGML と同じ形式に変換する
            var plateauGmlPath = Path.Combine(outputDirectory, $"{meshCode}_bldg_6697_2_op.gml");
            var appearanceDirectoryName = $"{meshCode}_bldg_6697_appearance";
            await ApplyPlateauCityGmlAsync(filePath, plateauGmlPath, envelope, appearanceDirectoryName);
            File.Delete(filePath);

            // appearance フォルダを PLATEAU と同じ形式に変更
            var sourceAppearancePath = Path.Combine(outputDirectory, "appearance");
            var plateauAppearancePath = Path.Combine(outputDirectory, appearanceDirectoryName);
            if (Directory.Exists(sourceAppearancePath))
            {
                Directory.Move(sourceAppearancePath, Path.Combine(tempDirectory, plateauAppearancePath));
            }

            var zipFilePath = Path.ChangeExtension(tempDirectory, $"{meshCode}.zip");
            try
            {
                ZipFile.CreateFromDirectory(outputDirectory, zipFilePath, CompressionLevel.Optimal, false);

                var memoryStream = new MemoryStream();
                using (var fileStream = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read))
                {
                    await fileStream.CopyToAsync(memoryStream);
                }
                memoryStream.Position = 0;

                return memoryStream;
            }
            finally
            {
                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }
            }
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }
    }

    private static async Task ApplyPlateauCityGmlAsync(string inputPath, string outputPath, Geometry envelope, string plateauAppearancePath)
    {
        var xmlReaderSettings = new XmlReaderSettings
        {
            Async = true
        };
        var xmlWriterSettings = new XmlWriterSettings()
        {
            Indent = true,
            NewLineChars = "\n",
            Encoding = new System.Text.UTF8Encoding(false),
            Async = true,
        };
        using (XmlReader reader = XmlReader.Create(inputPath, xmlReaderSettings))
        using (XmlWriter writer = XmlWriter.Create(outputPath, xmlWriterSettings))
        {
            bool insideCityModel = false;
            bool boundedByWritten = false;
            bool isImageUri = false;
            bool isMimeType = false;

            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.XmlDeclaration)
                {
                    await writer.WriteStartDocumentAsync();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "CityModel")
                {
                    // PLATEAU CityGMLの名前空間と属性を設定
                    await writer.WriteStartElementAsync("core", reader.LocalName, NamespaceCityGml2_0);
                    await writer.WriteAttributeStringAsync("xmlns", "brid", null, "http://www.opengis.net/citygml/bridge/2.0");
                    await writer.WriteAttributeStringAsync("xmlns", "tran", null, "http://www.opengis.net/citygml/transportation/2.0");
                    await writer.WriteAttributeStringAsync("xmlns", "frn", null, "http://www.opengis.net/citygml/cityfurniture/2.0");
                    await writer.WriteAttributeStringAsync("xmlns", "wtr", null, "http://www.opengis.net/citygml/waterbody/2.0");
                    await writer.WriteAttributeStringAsync("xmlns", "sch", null, "http://www.ascc.net/xml/schematron");
                    await writer.WriteAttributeStringAsync("xmlns", "veg", null, "http://www.opengis.net/citygml/vegetation/2.0");
                    await writer.WriteAttributeStringAsync("xmlns", "xlink", null, "http://www.w3.org/1999/xlink");
                    await writer.WriteAttributeStringAsync("xmlns", "tun", null, "http://www.opengis.net/citygml/tunnel/2.0");
                    await writer.WriteAttributeStringAsync("xmlns", "tex", null, "http://www.opengis.net/citygml/texturedsurface/2.0");
                    await writer.WriteAttributeStringAsync("xmlns", "gml", null, "http://www.opengis.net/gml");
                    await writer.WriteAttributeStringAsync("xmlns", "app", null, "http://www.opengis.net/citygml/appearance/2.0");
                    await writer.WriteAttributeStringAsync("xmlns", "gen", null, "http://www.opengis.net/citygml/generics/2.0");
                    await writer.WriteAttributeStringAsync("xmlns", "dem", null, "http://www.opengis.net/citygml/relief/2.0");
                    await writer.WriteAttributeStringAsync("xmlns", "luse", null, "http://www.opengis.net/citygml/landuse/2.0");
                    await writer.WriteAttributeStringAsync("xmlns", "uro", null, "https://www.geospatial.jp/iur/uro/3.0");
                    await writer.WriteAttributeStringAsync("xmlns", "xAL", null, "urn:oasis:names:tc:ciq:xsdschema:xAL:2.0");
                    await writer.WriteAttributeStringAsync("xmlns", "bldg", null, "http://www.opengis.net/citygml/building/2.0");
                    await writer.WriteAttributeStringAsync("xmlns", "smil20", null, "http://www.w3.org/2001/SMIL20/");
                    await writer.WriteAttributeStringAsync("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                    await writer.WriteAttributeStringAsync("xmlns", "smil20lang", null, "http://www.w3.org/2001/SMIL20/Language");
                    await writer.WriteAttributeStringAsync("xmlns", "pbase", null, "http://www.opengis.net/citygml/profiles/base/2.0");
                    await writer.WriteAttributeStringAsync("xmlns", "core", null, NamespaceCityGml2_0);
                    await writer.WriteAttributeStringAsync("xmlns", "grp", null, "http://www.opengis.net/citygml/cityobjectgroup/2.0");
                    await writer.WriteAttributeStringAsync("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", CityGmlSchemaLocation);

                    reader.MoveToElement();

                    // フラグ立ててboundedBy挿入タイミングとする
                    insideCityModel = true;
                    boundedByWritten = false;
                }
                else if (insideCityModel && !boundedByWritten)
                {
                    // 最初の子ノードの前にboundedByを挿入
                    await WriteBoundedBy(writer, envelope);
                    boundedByWritten = true;

                    // 先読みしたノードを書き戻す
                    await WriteShallowNode(reader, writer);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "imageURI")
                {
                    isImageUri = true;
                    await WriteShallowNode(reader, writer);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "mimeType")
                {
                    isMimeType = true;
                    await WriteShallowNode(reader, writer);
                }
                else
                {
                    if (isImageUri)
                    {
                        await WriteImageURI(reader, writer, plateauAppearancePath);
                    }
                    else if (isMimeType)
                    {
                        await WriteMimeType(reader, writer);
                    }
                    else
                    {
                        await WriteShallowNode(reader, writer);
                    }
                }

                // CityModelの終了タグに達したらリセット
                if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "CityModel")
                {
                    insideCityModel = false;
                    boundedByWritten = false;
                }

                // imageURIの終了タグに達したらリセット
                if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "imageURI")
                {
                    isImageUri = false;
                    await WriteShallowNode(reader, writer);
                }

                // mimeTypeの終了タグに達したらリセット
                if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "mimeType")
                {
                    isMimeType = false;
                    await WriteShallowNode(reader, writer);
                }
            }
        }
    }

    private static async Task WriteBoundedBy(XmlWriter writer, Geometry envelope)
    {
        var coordinates = envelope.Coordinates;
        await writer.WriteStartElementAsync("gml", "boundedBy", "http://www.opengis.net/gml");

        await writer.WriteStartElementAsync("gml", "Envelope", "http://www.opengis.net/gml");
        await writer.WriteAttributeStringAsync(null, "srsName", null, "http://www.opengis.net/def/crs/EPSG/0/6697");
        await writer.WriteAttributeStringAsync(null, "srsDimension", null, "3");

        await writer.WriteElementStringAsync("gml", "lowerCorner", "http://www.opengis.net/gml",
            $"{coordinates.Min(x => x.Y)} {coordinates.Min(x => x.X)} {coordinates.Min(x => x.Z)}");
        await writer.WriteElementStringAsync("gml", "upperCorner", "http://www.opengis.net/gml",
            $"{coordinates.Max(x => x.Y)} {coordinates.Max(x => x.X)} {coordinates.Max(x => x.Z)}");

        await writer.WriteEndElementAsync(); // Envelope
        await writer.WriteEndElementAsync(); // boundedBy
    }

    private static async Task WriteShallowNode(XmlReader reader, XmlWriter writer)
    {
        switch (reader.NodeType)
        {
            case XmlNodeType.Element:
                await writer.WriteStartElementAsync(reader.NamespaceURI == NamespaceCityGml2_0 ? "core" : reader.Prefix, reader.LocalName, reader.NamespaceURI);
                if (reader.HasAttributes)
                {
                    while (reader.MoveToNextAttribute())
                        await writer.WriteAttributeStringAsync(reader.Prefix, reader.LocalName, reader.NamespaceURI, reader.Value);
                    reader.MoveToElement();
                }

                if (reader.IsEmptyElement)
                {
                    await writer.WriteEndElementAsync();
                }
                break;

            case XmlNodeType.Text:
                await writer.WriteStringAsync(reader.Value);
                break;

            case XmlNodeType.CDATA:
                await writer.WriteCDataAsync(reader.Value);
                break;
            case XmlNodeType.EndElement:
                await writer.WriteEndElementAsync();
                break;

            case XmlNodeType.ProcessingInstruction:
            case XmlNodeType.Comment:
            case XmlNodeType.Whitespace:
            case XmlNodeType.SignificantWhitespace:
                break;
        }
    }

    private static async Task WriteImageURI(XmlReader reader, XmlWriter writer, string plateauAppearancePath)
    {
        await writer.WriteStringAsync(reader.Value.Replace("appearance/", $"{plateauAppearancePath}/"));
    }

    private static async Task WriteMimeType(XmlReader reader, XmlWriter writer)
    {
        await writer.WriteStringAsync(reader.Value.Replace("jpg", "jpeg"));
        {
            await writer.WriteStringAsync(reader.Value.Replace("image/jpg", "image/jpeg"));
        }
    }
}
