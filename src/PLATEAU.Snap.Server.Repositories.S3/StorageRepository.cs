using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using PLATEAU.Snap.Models.Server;
using System.Net;
using System.Text.RegularExpressions;

namespace PLATEAU.Snap.Server.Repositories;

internal class StorageRepository : IStorageRepository
{
    private static readonly Regex pattern = new Regex(@"^s3://(?<bucket>[^/]+)/(?<key>.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

    public async Task<byte[]> DownloadAsync(string path)
    {
        var match = pattern.Match(path);
        if (!match.Success)
        {
            throw new ArgumentException($"Invalid S3 path format: {path}", nameof(path));
        }

        var request = new GetObjectRequest
        {
            BucketName = match.Groups["bucket"].Value,
            Key = match.Groups["key"].Value
        };

        using var response = await amazonS3.GetObjectAsync(request);
        var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream);

        return memoryStream.ToArray();
    }

    public async Task<string> GeneratePreSignedURLAsync(string path, int expiryInMinutes)
    {
        var match = pattern.Match(path);
        if (!match.Success)
        {
            throw new ArgumentException($"Invalid S3 path format: {path}", nameof(path));
        }

        var request = new GetPreSignedUrlRequest
        {
            BucketName = match.Groups["bucket"].Value,
            Key = match.Groups["key"].Value,
            Expires = DateTime.UtcNow.AddMinutes(expiryInMinutes),
            Verb = HttpVerb.GET
        };

        return await amazonS3.GetPreSignedURLAsync(request);
    }
}
