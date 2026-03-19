using PLATEAU.Snap.Models;

namespace PLATEAU.Snap.Server.Entities.Models;

public partial class Job
{
    private const int ExpiryInMinutes = 60;

    public Snap.Models.Client.Job ToClientModel()
    {
        var type = Enum.Parse<Snap.Models.Common.JobType>(this.Type);
        return new Snap.Models.Client.Job
        {
            Id = this.Id,
            Type = type,
            Status = Enum.Parse<Snap.Models.Common.JobStatusType>(this.Status),
            Parameter = ToJobParam(type, this.Parameter),
            Message = this.Message,
            ResultParameter = ToJobResultParam(type, this.ResultParameter, null),
            CreatedAt = this.CreatedAt,
            UpdatedAt = this.UpdatedAt,
        };
    }

    public Snap.Models.Client.Job ToClientModelResolvePath(Func<string, int, Task<string>> generatePreSignedURLAsync)
    {
        var type = Enum.Parse<Snap.Models.Common.JobType>(this.Type);
        return new Snap.Models.Client.Job
        {
            Id = this.Id,
            Type = type,
            Status = Enum.Parse<Snap.Models.Common.JobStatusType>(this.Status),
            Parameter = ToJobParam(type, this.Parameter),
            ResultParameter = ToJobResultParam(type, this.ResultParameter, generatePreSignedURLAsync),
            Message = this.Message,
            CreatedAt = this.CreatedAt,
            UpdatedAt = this.UpdatedAt,
        };
    }

    private static Snap.Models.Common.AbstractJobParam? ToJobParam(Snap.Models.Common.JobType type, string? value)
    {
        if (value == null)
        {
            return null;
        }
        return type switch
        {
            Snap.Models.Common.JobType.export_building => Util.Deserialize<Snap.Models.Common.ExportBuildingParam>(value),
            Snap.Models.Common.JobType.export_mesh => Util.Deserialize<Snap.Models.Common.ExportMeshParam>(value),
            _ => throw new NotImplementedException($"Unknown job type: {type}"),
        };
    }

    private static Snap.Models.Common.AbstractJobResultParam? ToJobResultParam(Snap.Models.Common.JobType type, string? value, Func<string, int, Task<string>>? generatePreSignedURLAsync)
    {
        if (value == null)
        {
            return null;
        }
        switch (type)
        {
            case Snap.Models.Common.JobType.export_building:
                {
                    var result = Util.Deserialize<Snap.Models.Common.ExportBuildingResultParam>(value);
                    if (result?.Path != null && generatePreSignedURLAsync is not null)
                    {
                        result.Path = generatePreSignedURLAsync(result.Path, ExpiryInMinutes).Result;
                    }

                    return result;
                }
            case Snap.Models.Common.JobType.export_mesh:
                {
                    var result = Util.Deserialize<Snap.Models.Common.ExportMeshResultParam>(value);
                    if (result?.Path != null && generatePreSignedURLAsync is not null)
                    {
                        result.Path = generatePreSignedURLAsync(result.Path, ExpiryInMinutes).Result;
                    }

                    return result;
                }
            default:
                throw new NotImplementedException($"Unknown job type: {type}");
        };
    }
}
