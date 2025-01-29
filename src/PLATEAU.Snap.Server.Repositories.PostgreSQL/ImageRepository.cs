using Microsoft.EntityFrameworkCore;
using PLATEAU.Snap.Server.Entities;
using PLATEAU.Snap.Server.Entities.Models;
using System.Data;
using System.Net;

namespace PLATEAU.Snap.Server.Repositories;

internal class ImageRepository : BaseRepository, IImageRepository
{
    private readonly IStorageRepository storage;

    public ImageRepository(CitydbV4DbContext dbContext, IStorageRepository storage) : base(dbContext)
    {
        this.storage = storage;
    }

    public async Task<Image> CreateAsync(Image image, Stream stream)
    {
        using var connection = this.Context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT nextval('images_id_seq'::regclass)";
#pragma warning disable CS8605
        var id = (long)await command.ExecuteScalarAsync();
#pragma warning restore CS8605

        var response = await this.storage.UploadAsync(stream, $"{ DateTime.Now.ToString("yyyy-MM-dd")}/{id}.png");
        if (response.StatusCode != HttpStatusCode.OK || response.Uri is null)
        {
            // TODO: 投げる例外の精査
            throw new Exception("Failed to upload image");
        }

        image.Id = id;
        image.Uri = response.Uri;
        Context.Images.Add(image);
        await Context.SaveChangesAsync();

        return image;
    }
}
