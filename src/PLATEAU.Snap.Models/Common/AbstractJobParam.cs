using PLATEAU.Snap.Models.Json;
using System.Text.Json.Serialization;

namespace PLATEAU.Snap.Models.Common;

[JsonConverter(typeof(JobParamConverter))]
abstract public class AbstractJobParam
{
    abstract public JobType Type { get; }
}
