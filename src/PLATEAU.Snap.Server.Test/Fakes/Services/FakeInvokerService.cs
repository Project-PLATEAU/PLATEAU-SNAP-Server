using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using PLATEAU.Snap.Models.Lambda;
using PLATEAU.Snap.Server.Services;

namespace PLATEAU.Snap.Server.Test.Fakes.Services;

internal class FakeInvokerService : IInvokerService
{
    private GeometryFactory geometryFactory = new ();

    private WKTWriter wktWriter = new ();

    public Task<LambdaTransformResponse> TransformAsync(LambdaTransformRequest request)
    {
        var polygon = geometryFactory.CreatePolygon(
        [
            new Coordinate(10, 10),
            new Coordinate(10, 20),
            new Coordinate(20, 20),
            new Coordinate(20, 10),
            new Coordinate(10, 10),
        ]);

        return Task.FromResult(new LambdaTransformResponse
        {
            Path = "s3://temp/transform.png",
            Coordinates = wktWriter.Write(polygon),
        });
    }

    public Task<LambdaRoofExtractionResponse> RoofExtractionAsync(LambdaRoofExtractionRequest request)
    {
        var polygon = geometryFactory.CreatePolygon(
        [
            new Coordinate(10, 10),
            new Coordinate(10, 20),
            new Coordinate(20, 20),
            new Coordinate(20, 10),
            new Coordinate(10, 10),
        ]);

        return Task.FromResult(new LambdaRoofExtractionResponse
        {
            Path = "s3://temp/roof_extraction.png",
            Coordinates = wktWriter.Write(polygon),
        });
    }

    public Task<LambdaApplyTextureResponse> ApplyTextureAsync(LambdaApplyTextureRequest request)
    {
        var polygon = geometryFactory.CreatePolygon(
        [
            new Coordinate(10, 10),
            new Coordinate(10, 20),
            new Coordinate(20, 20),
            new Coordinate(20, 10),
            new Coordinate(10, 10),
        ]);

        return Task.FromResult(new LambdaApplyTextureResponse
        {
            Path = "s3://temp/apply_texture.png",
            TextureCoordinates = wktWriter.Write(polygon),
        });
    }

    public async Task ExportBuildingAsync(LambdaExportBuildingRequest request)
    {
        await Task.CompletedTask;
    }

    public async Task ExportMeshAsync(LambdaExportMeshRequest request)
    {
        await Task.CompletedTask;
    }
}
