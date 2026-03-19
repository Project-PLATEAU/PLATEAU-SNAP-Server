using PLATEAU.Snap.Models.Json;
using System.Text.Json.Serialization;

namespace PLATEAU.Snap.Models.Common;

[JsonConverter(typeof(JobResultParamConverter))]
abstract public class AbstractJobResultParam
{
    abstract public JobType Type { get; }
}
