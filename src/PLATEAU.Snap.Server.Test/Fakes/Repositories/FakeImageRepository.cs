using PLATEAU.Snap.Server.Entities.Models;
using PLATEAU.Snap.Server.Repositories;

namespace PLATEAU.Snap.Server.Test.Fakes.Repositories;

internal class FakeImageRepository : IImageRepository
{
    public List<Textureparam> Textureparams { get; } = new ();

    public bool IsTextureparamNull { get; set; }

    public bool IsTexImageNull { get; set; }

    public bool HasFace { get; set; } = true;

    public int RelationSurfaceDataCount { get; set; } = 1;

    public Task<Image> CreateAsync(Image image, Stream stream)
    {
        throw new NotImplementedException();
    }

    public Task<byte[]> DownloadAsync(string path)
    {
        return Task.FromResult(new byte[] { 0x01, 0x02, 0x03 });
    }

    public Task<string> GeneratePreSignedURLAsync(string path, int expiryInMinutes)
    {
        return Task.FromResult($"https://example.com/{path.Replace("s3://", "")}?expiry={expiryInMinutes}");
    }

    public async Task<Textureparam?> GetTextureparamAsync(int surfaceGeometryId)
    {
        if (IsTextureparamNull)
        {
            return await Task.FromResult<Textureparam?>(null);
        }

        var textureparam = Textureparams.FirstOrDefault(tp => tp.SurfaceGeometryId == surfaceGeometryId);
        if (IsTexImageNull && textureparam != null)
        {
            textureparam.SurfaceData.TexImage = null;
        }
        return await Task.FromResult(textureparam);
    }

    public async Task<bool> FaceExists(int surfaceGeometryId)
    {
        return await Task.FromResult(HasFace);
    }

    public async Task<int> CountTextureparam(int texImageId)
    {
        return await Task.FromResult(RelationSurfaceDataCount);
    }

    public async Task UpdateTextureparamAsync(Textureparam textureparam)
    {
        await Task.CompletedTask;
    }

    public async Task AddTextureparamAsync(Textureparam textureparam)
    {
        await Task.CompletedTask;
    }

    public async Task AddSurfaceData(SurfaceDatum surfaceData, Textureparam textureparam)
    {
        await Task.CompletedTask;
    }

    public async Task<Objectclass?> GetObjectClass(string classname)
    {
        return await Task.FromResult(new Objectclass() { Id = 54 });
    }

    public Task<Appearance?> GetAppearanceAsync(int buildingId)
    {
        return Task.FromResult<Appearance?>(new Appearance()
        {
            Id = 1,
            Gmlid = "appearance-1",
            Name = "Test Appearance",
        });
    }
}
