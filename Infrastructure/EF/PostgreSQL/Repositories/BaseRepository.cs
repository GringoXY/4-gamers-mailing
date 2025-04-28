using Contracts.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Shared.Repositories;
using Shared.Entities;

namespace Infrastructure.EF.PostgreSQL.Repositories;

internal class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public BaseRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id)
        => await _dbSet.FindAsync(id);

    public virtual async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate)
        => await _dbSet.Where(predicate).ToListAsync();

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        => await _dbSet.ToListAsync();

    public async Task<bool> ExistsAsync(Guid id)
        => await _dbSet.AnyAsync(e => e.Id == id);

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
        => await _dbSet.AnyAsync(predicate);

    public virtual async Task AddAsync(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public virtual void Add(TEntity entity)
    {
        _dbSet.Add(entity);
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        _dbSet.AddRange(entities);
        await _context.SaveChangesAsync();
    }

    public virtual void AddRange(IEnumerable<TEntity> entities)
        => _dbSet.AddRange(entities);

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity is null || entity.IsDeleted)
        {
            return;
        }

        await DeleteAsync(entity);
    }

    public virtual async Task DeleteAsync(TEntity entity)
    {
        entity.SetPropertyValue(nameof(BaseEntity.DeletedAt), DateTime.UtcNow);
        _dbSet.Update(entity);
        _context.Entry(entity).State = EntityState.Deleted;
        await _context.SaveChangesAsync();
    }

    public virtual void Delete(Guid id)
    {
        var entity = _dbSet.Find(id);
        if (entity is null || entity.IsDeleted)
        {
            return;
        }

        Delete(entity);
    }

    public virtual void Delete(TEntity entity)
    {
        entity.SetPropertyValue(nameof(BaseEntity.DeletedAt), DateTime.UtcNow);
        _dbSet.Update(entity);
        _context.Entry(entity).State = EntityState.Deleted;
    }

    public virtual async Task UpdateAsync(TEntity entity)
    {
        entity.SetPropertyValue(nameof(BaseEntity.UpdatedAt), DateTime.UtcNow);
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual void Update(TEntity entity)
    {
        entity.SetPropertyValue(nameof(BaseEntity.UpdatedAt), DateTime.UtcNow);
        _dbSet.Update(entity);
    }

    public virtual void UpdateRange(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            entity.SetPropertyValue(nameof(BaseEntity.UpdatedAt), DateTime.UtcNow);
        }

        _dbSet.UpdateRange(entities);
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            entity.SetPropertyValue(nameof(BaseEntity.UpdatedAt), DateTime.UtcNow);
        }

        _dbSet.UpdateRange(entities);
        await _context.SaveChangesAsync();
    }
}
