using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using PLATEAU.Snap.Models;
using PLATEAU.Snap.Models.Client;
using PLATEAU.Snap.Models.Common;
using PLATEAU.Snap.Models.Exceptions;
using PLATEAU.Snap.Models.Settings;
using PLATEAU.Snap.Server.Entities.Models;
using PLATEAU.Snap.Server.Repositories;

namespace PLATEAU.Snap.Server.Services;

internal class TextureService : ITextureService
{
    private const int ExpiryInMinutes = 60;

    private readonly IImageRepository imageRepository;

    private readonly IJobRepository jobRepository;

    private readonly IInvokerService invokerService;

    private readonly ISurfaceGeometryRepository surfaceGeometryRepository;

    public TextureService(IImageRepository imageRepository, IJobRepository jobRepository, IInvokerService invokerService, ISurfaceGeometryRepository surfaceGeometryRepository, AppSettings appSettings, DatabaseSettings databaseSettings)
    {
        this.imageRepository = imageRepository;
        this.jobRepository = jobRepository;
        this.invokerService = invokerService;
        this.surfaceGeometryRepository = surfaceGeometryRepository;
    }

    public async Task<PageData<BuildingImage>> GetBuildingsAsync(SortType sortType, int pageNumber, int pageSize)
    {
        var pageList = await this.surfaceGeometryRepository.GetBuildingsAsync(sortType, pageNumber, pageSize);
        return pageList.CreatePageData();
    }

    public async Task<MeshCodeResponse> GetMeshCodeAsync(int buildingId)
    {
        var roofprint = await this.surfaceGeometryRepository.GetRoofprintAsync(buildingId);
        if (roofprint is null)
        {
            throw new NotFoundException($"Building with ID {buildingId} does not exist.");
        }
        var meshCode = MeshUtil.GetThirdMeshCode(roofprint);

        return new MeshCodeResponse() { MeshCode = meshCode };
    }

    public async Task<PageData<FaceImageInfo>> GetFacesAsync(int buildingId, SortType sortType, int pageNumber, int pageSize)
    {
        if (!(await this.surfaceGeometryRepository.ExistsAsync(buildingId)))
        {
            throw new NotFoundException();
        }

        var pageList = await this.surfaceGeometryRepository.GetFacesAsync(buildingId, sortType, pageNumber, pageSize);
        return pageList.CreatePageDataWithSelect(x => new FaceImageInfo(x.FaceId!.Value, x.Gmlid, x.Thumbnail, x.Timestamp, x.IsOrtho!.Value));
    }

    public async Task<PageData<ImageInfo>> GetFaceImagesAsync(int buildingId, int faceId, SortType sortType, int pageNumber, int pageSize)
    {
        if (!(await this.surfaceGeometryRepository.ExistsAsync(buildingId, faceId)))
        {
            throw new NotFoundException();
        }

        var pageList = await this.surfaceGeometryRepository.GetFaceImagesAsync(buildingId, faceId, sortType, pageNumber, pageSize);
        return pageList.CreatePageDataWithSelect(x => new ImageInfo(x.ImageId, x.Gmlid, x.Thumbnail, x.Timestamp, x.IsOrtho!.Value));
    }

    public async Task<Models.Client.TransformResponse> TransformAsync(Models.Client.TransformRequest payload)
    {
        var surfaceImage = await this.surfaceGeometryRepository.GetSurfaceImageAsync(payload.BuildingId, payload.FaceId, payload.ImageId);
        if (surfaceImage is null)
        {
            throw new NotFoundException();
        }

        // アスペクト比の計算にジオイド高は考慮する必要がないため、ここでは無視する
        var wkt = await this.surfaceGeometryRepository.GetFaceWktAsync(payload.FaceId);
        if (wkt is null)
        {
            throw new InvalidOperationException("The face geometry is not available.");
        }

        var writer = new WKTWriter();
        var coordinates = writer.Write(surfaceImage.Coordinates);

        var response = await invokerService.TransformAsync(new Models.Lambda.LambdaTransformRequest()
        {
            Path = surfaceImage.Uri!,
            Coordinates = coordinates,
            Geometry = wkt
        });

        var preSignedURL = await this.imageRepository.GeneratePreSignedURLAsync(response.Path, ExpiryInMinutes);

        return new Models.Client.TransformResponse(response.Path, preSignedURL, response.Coordinates);
    }

    public async Task<Models.Client.RoofExtractionResponse> RoofExtractionAsync(Models.Client.RoofExtractionRequest payload)
    {
        var roofSurface = await this.surfaceGeometryRepository.GetRoofSurfaceAsync(payload.BuildingId, payload.FaceId);
        if (roofSurface is null)
        {
            throw new NotFoundException();
        }

        var writer = new WKTWriter();
        var geometry = writer.Write(roofSurface.Geom);

        var response = await invokerService.RoofExtractionAsync(new Models.Lambda.LambdaRoofExtractionRequest()
        {
            Geometry = geometry
        });

        var preSignedURL = await this.imageRepository.GeneratePreSignedURLAsync(response.Path, ExpiryInMinutes);

        return new Models.Client.RoofExtractionResponse(response.Path, preSignedURL, response.Coordinates);
    }

    public async Task<PreviewTextureResponse> PreviewTextureRequest(PreviewTextureRequest payload)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("PreviewTextureRequest is not implemented yet.");
    }

    public async Task ApplyTextureAsync(ApplyTextureRequest payload)
    {
        if (!(await this.imageRepository.FaceExists(payload.FaceId)))
        {
            throw new NotFoundException($"Face with ID {payload.FaceId} does not exist.");
        }

        var textureparam = await this.imageRepository.GetTextureparamAsync(payload.FaceId);

        var response = await invokerService.ApplyTextureAsync(new Models.Lambda.LambdaApplyTextureRequest()
        {
            Path = payload.Path,
            Coordinates = payload.Coordinates,
        });

        var reader = new WKTReader();
        var textureCoordinates = reader.Read(response.TextureCoordinates) as Polygon ?? throw new InvalidOperationException($"{nameof(response.TextureCoordinates)} is not a valid polygon.");
        var fileBytes = await this.imageRepository.DownloadAsync(response.Path);
        string? mimeType = null;
        string extension = Path.GetExtension(response.Path).ToLower();
        switch (extension)
        {
            case ".png":
                mimeType = "image/png";
                break;
            case ".jpg":
            case ".jpeg":
                mimeType = "image/jpeg";
                break;
            default:
                throw new InvalidOperationException($"Unsupported texture file type: {Path.GetExtension(response.Path)}");
        }

        if (textureparam is null)
        {
            // Textureparamが存在しない場合は新規追加
            await AddTextureparamAsync(payload.FaceId, textureCoordinates, mimeType, extension, fileBytes, payload.BuildingId);
        }
        else
        {
            if (textureparam.SurfaceData.TexImage is null)
            {
                throw new InvalidOperationException("SurfaceData is null in Textureparam.");
            }

            var count = await this.imageRepository.CountTextureparam(textureparam.SurfaceData.TexImage.Id);
            if (count == 1)
            {
                // このTexImageに紐づくTextureparamが1つだけの場合は更新
                await UpdateTexImageAsync(textureparam, textureCoordinates, mimeType, extension, fileBytes, payload.BuildingId);
            }
            else if (count > 1)
            {
                // このTexImageに紐づくTextureparamが複数ある場合は新規追加
                await AddTexImageAsync(textureparam, textureCoordinates, mimeType, extension, fileBytes, payload.BuildingId);
            }
            else
            {
                throw new InvalidOperationException("Inconsistent state: No Textureparam found for the given TexImage.");
            }
        }
    }

    public async Task<Models.Client.Job> ExportAsync(int buildingId, string? fileName)
    {
        using var transaction = await this.jobRepository.BeginTransactionAsync();
        try
        {
            var job = await this.jobRepository.AddAsync(new Entities.Models.Job
            {
                Type = JobType.export_building.ToString(),
                Status = JobStatusType.pending.ToString(),
                Parameter = Util.Serialize(new ExportBuildingParam
                {
                    BuildingId = buildingId,
                    FileName = fileName,
                }),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            });

            await this.invokerService.ExportBuildingAsync(new Models.Lambda.LambdaExportBuildingRequest
            {
                JobId = job.Id,
                BuildingId = buildingId,
            });

            await transaction.CommitAsync();

            return job.ToClientModel();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Models.Client.Job> ExportAsync(string meshCode, string? fileName)
    {
        using var transaction = await this.jobRepository.BeginTransactionAsync();
        try
        {
            var job = await this.jobRepository.AddAsync(new Entities.Models.Job
            {
                Type = JobType.export_mesh.ToString(),
                Status = JobStatusType.pending.ToString(),
                Parameter = Util.Serialize(new ExportMeshParam
                {
                    MeshCode = meshCode,
                    FileName = fileName,
                }),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            });

            await this.invokerService.ExportMeshAsync(new Models.Lambda.LambdaExportMeshRequest
            {
                JobId = job.Id,
                MeshCode = meshCode,
            });

            await transaction.CommitAsync();

            return job.ToClientModel();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task AddTextureparamAsync(int faceId, Polygon textureCoordinates, string mimeType, string extension, byte[] fileBytes, int buildingId)
    {
        var objectClass = await this.imageRepository.GetObjectClass("ParameterizedTexture");
        if (objectClass is null)
        {
            throw new InvalidOperationException("Object class 'ParameterizedTexture' not found.");
        }

        var appearance = await this.imageRepository.GetAppearanceAsync(buildingId);
        if (appearance is null)
        {
            throw new InvalidOperationException($"Building with ID {buildingId} does not exist.");
        }

        var textureparam = new Textureparam
        {
            SurfaceGeometryId = faceId,
            IsTextureParametrization = 1,
            TextureCoordinates = textureCoordinates,
            SurfaceData = new SurfaceDatum
            {
                Gmlid = $"ID_{Guid.NewGuid()}",
                IsFront = 1,
                ObjectclassId = objectClass.Id,
                TexImage = new TexImage
                {
                    TexMimeType = mimeType,
                    TexImageData = fileBytes,
                    TexImageUri = $"tex_{Guid.NewGuid()}{extension}",
                },
                Appearances = new List<Appearance> { appearance }
            }
        };

        await this.imageRepository.AddTextureparamAsync(textureparam);
    }

    private async Task UpdateTexImageAsync(Textureparam textureparam, Polygon textureCoordinates, string mimeType, string extension, byte[] fileBytes, int buildingId)
    {
        if (textureparam.SurfaceData.TexImage is null)
        {
            throw new InvalidOperationException("TexImage is null in Textureparam.");
        }

        textureparam.TextureCoordinates = textureCoordinates;
        textureparam.SurfaceData.TexImage.TexMimeType = mimeType;
        textureparam.SurfaceData.TexImage.TexImageData = fileBytes;
        textureparam.SurfaceData.TexImage.TexImageUri = !string.IsNullOrEmpty(textureparam.SurfaceData.TexImage.TexImageUri)
            ? Path.ChangeExtension(textureparam.SurfaceData.TexImage.TexImageUri, extension)
            : $"tex_{textureparam.SurfaceData.TexImage.Id}{extension}";

        await this.imageRepository.UpdateTextureparamAsync(textureparam);
    }

    private async Task AddTexImageAsync(Textureparam textureparam, Polygon textureCoordinates, string mimeType, string extension, byte[] fileBytes, int buildingId)
    {
        var appearance = await this.imageRepository.GetAppearanceAsync(buildingId);
        if (appearance is null)
        {
            throw new InvalidOperationException($"Building with ID {buildingId} does not exist.");
        }

        var surfaceData = new SurfaceDatum
        {
            Gmlid = $"ID_{Guid.NewGuid()}",
            IsFront = 1,
            ObjectclassId = textureparam.SurfaceData.ObjectclassId,
            TexImage = new TexImage
            {
                TexMimeType = mimeType,
                TexImageData = fileBytes,
                TexImageUri = $"tex_{Guid.NewGuid()}{extension}",
            },
            Appearances = new List<Appearance> { appearance }
        };

        await this.imageRepository.AddSurfaceData(surfaceData, textureparam);
    }
}
