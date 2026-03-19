using PLATEAU.Snap.Models.Server;
using PLATEAU.Snap.Server.Repositories;

namespace PLATEAU.Snap.Server.Test.Fakes.Repositories;

internal class FakeStorageRepository : IStorageRepository
{
    public Task<byte[]> DownloadAsync(string path)
    {
        throw new NotImplementedException();
    }

    public Task<string> GeneratePreSignedURLAsync(string path, int expiryInMinutes)
    {
        throw new NotImplementedException();
    }

    public Task<StorageUploadResponse> UploadAsync(Stream stream, string path)
    {
        throw new NotImplementedException();
    }
}
