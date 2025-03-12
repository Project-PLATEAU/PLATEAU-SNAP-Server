using PLATEAU.Snap.Models;
using PLATEAU.Snap.Models.Server;
using PLATEAU.Snap.Server.Entities.Models;
using PLATEAU.Snap.Server.Repositories;

namespace PLATEAU.Snap.Server.Services;

internal class ImageService : IImageService
{
    private readonly IImageRepository repository;

    public ImageService(IImageRepository repository)
    {
        this.repository = repository;
    }

    public async Task<Models.Client.BuildingImageResponse> CreateBuildingImageAsync(BuildingImageRequest request)
    {
        try
        {
            using var stream = request.File.OpenReadStream();
            var entity = await this.repository.CreateAsync(new Image(request.Metadata), stream);

            return new Models.Client.BuildingImageResponse()
            {
                Status = Models.Client.StatusType.Success,
                Id = entity.Id
            };
        }
        catch (SnapServerException ex)
        {
            return new Models.Client.BuildingImageResponse()
            {
                Status = Models.Client.StatusType.Error,
                Message = "File upload failed. Please try again.",
                Exception = ex
            };
        }
    }
}
