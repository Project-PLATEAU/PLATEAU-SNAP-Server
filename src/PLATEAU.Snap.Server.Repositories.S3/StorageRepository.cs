using Amazon.S3;
using Amazon.S3.Transfer;
using PLATEAU.Snap.Models.Server;
using System.Net;

namespace PLATEAU.Snap.Server.Repositories;

internal class StorageRepository : IStorageRepository
{
    private readonly IAmazonS3 amazonS3;

    private readonly S3Settings s3Settings;

    public StorageRepository(IAmazonS3 amazonS3, S3Settings s3Settings)
    {
        this.amazonS3 = amazonS3;
        this.s3Settings = s3Settings;
    }

    public async Task<StorageUploadResponse> UploadAsync(Stream stream, string path)
    {
        try
        {
            var transferUtility = new TransferUtility(this.amazonS3);
            var request = new TransferUtilityUploadRequest
            {
                BucketName = s3Settings.Bucket,
                Key = path,
                InputStream = stream,
            };
            await transferUtility.UploadAsync(request);

            var uri = $"s3://{s3Settings.Bucket}/{path}";
            return new StorageUploadResponse()
            {
                StatusCode = HttpStatusCode.OK,
                Uri = uri
            };
        }
        catch (AmazonS3Exception ex)
        {
            // TODO: ステータスコードの精査
            switch (ex.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    return new StorageUploadResponse(HttpStatusCode.BadRequest);
                case HttpStatusCode.Forbidden:
                    return new StorageUploadResponse(HttpStatusCode.Forbidden);
                default:
                    return new StorageUploadResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}
