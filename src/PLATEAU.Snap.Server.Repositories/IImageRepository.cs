using PLATEAU.Snap.Server.Entities.Models;

namespace PLATEAU.Snap.Server.Repositories;

public interface IImageRepository
{
    Task<Image> CreateAsync(Image image, Stream stream);
}
