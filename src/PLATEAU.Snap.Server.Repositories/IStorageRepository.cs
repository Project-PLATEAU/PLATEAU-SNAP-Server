using PLATEAU.Snap.Models.Server;

namespace PLATEAU.Snap.Server.Repositories;

public interface IStorageRepository
{
    Task<StorageUploadResponse> UploadAsync(Stream stream, string path);
}
