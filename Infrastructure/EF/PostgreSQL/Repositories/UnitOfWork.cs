using Microsoft.EntityFrameworkCore.Storage;
using Shared.Repositories;

namespace Infrastructure.EF.PostgreSQL.Repositories;

internal sealed class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    private readonly ApplicationDbContext _context = context;

    public Task<IDbContextTransaction> BeginTransactionAsync()
        => _context.Database.BeginTransactionAsync();

    public Task CompleteAsync()
        => _context.SaveChangesAsync();
}
