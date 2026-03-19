using PLATEAU.Snap.Models.Server;

namespace PLATEAU.Snap.Server.Repositories;

public interface IStorageRepository
{
    Task<StorageUploadResponse> UploadAsync(Stream stream, string path);

    Task<byte[]> DownloadAsync(string path);

    Task<string> GeneratePreSignedURLAsync(string path, int expiryInMinutes);
}
