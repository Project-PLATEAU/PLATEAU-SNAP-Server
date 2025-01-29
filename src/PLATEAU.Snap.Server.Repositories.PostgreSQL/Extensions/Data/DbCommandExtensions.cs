using System.Data.Common;
using System.Data;

namespace PLATEAU.Snap.Server.Repositories;

public static class DbCommandExtensions
{
    public static DbParameter CreateParameter(this IDbCommand command, string name, object value)
    {
        var param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        return (DbParameter)param;
    }
}
