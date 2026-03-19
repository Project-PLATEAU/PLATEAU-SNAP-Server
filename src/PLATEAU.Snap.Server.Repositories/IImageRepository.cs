using PLATEAU.Snap.Server.Entities.Models;

namespace PLATEAU.Snap.Server.Repositories;

public interface IImageRepository
{
    Task<Image> CreateAsync(Image image, Stream stream);

    Task<string> GeneratePreSignedURLAsync(string path, int expiryInMinutes);

    Task<byte[]> DownloadAsync(string path);

    Task<Textureparam?> GetTextureparamAsync(int surfaceGeometryId);

    Task<bool> FaceExists(int surfaceGeometryId);

    Task<int> CountTextureparam(int texImageId);

    Task UpdateTextureparamAsync(Textureparam textureparam);

    Task AddTextureparamAsync(Textureparam textureparam);

    Task AddSurfaceData(SurfaceDatum surfaceData, Textureparam textureparam);

    Task<Objectclass?> GetObjectClass(string classname);

    Task<Appearance?> GetAppearanceAsync(int buildingId);
}
