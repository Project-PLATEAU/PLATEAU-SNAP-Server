using Microsoft.EntityFrameworkCore.Storage;

namespace PLATEAU.Snap.Server.Repositories;

public interface IBaseRepository
{
    Task<IDbContextTransaction> BeginTransactionAsync();
}
