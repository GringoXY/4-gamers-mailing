using Microsoft.EntityFrameworkCore.Storage;

namespace Shared.Repositories;

public interface IUnitOfWork
{
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task CompleteAsync();
}
