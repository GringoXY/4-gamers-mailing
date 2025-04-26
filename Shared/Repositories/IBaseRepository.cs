using Shared.Entities;
using System.Linq.Expressions;

namespace Shared.Repositories;

/// <summary>
/// Represents an interface for base repository
/// </summary>
/// <typeparam name="TEntity">Class entity type for instance <see cref="Entities.User.User"/></typeparam>
public interface IBaseRepository<TEntity> where TEntity : BaseEntity
{
    /// <summary>
    /// Asynchronously gets entity record
    /// </summary>
    /// <param name="id">The entity's id</param>
    /// <returns>Entity record</returns>
    Task<TEntity?> GetByIdAsync(Guid id);

    /// <summary>
    /// Asynchronously gets/filters out entities records by provided <see href="predicate" />
    /// </summary>
    /// <param name="predicate">Filters</param>
    /// <returns>List of entities</returns>
    Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Asynchronously gets all entities
    /// </summary>
    /// <returns>List of entities</returns>
    Task<IEnumerable<TEntity>> GetAllAsync();

    /// <summary>
    /// Asynchronously checks whether entity with specified <see cref="BaseEntity.Id"/> exists in DB
    /// </summary>
    /// <returns><c>true</c> when entity exists; otherwise <c>false</c></returns>
    Task<bool> ExistsAsync(Guid id);

    /// <summary>
    /// Asynchronously checks whether entity with specific values exists in DB
    /// </summary>
    /// <returns><c>true</c> when entity exists; otherwise <c>false</c></returns>
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Asynchronously adds new entity record
    /// </summary>
    /// <param name="entity">The entity/record to add</param>
    Task AddAsync(TEntity entity);

    /// <summary>
    /// Synchronously adds new entity record
    /// </summary>
    /// <param name="entity">The entity/record to add</param>
    void Add(TEntity entity);

    /// <summary>
    /// Asynchronously adds new entities (records)
    /// </summary>
    /// <param name="entities">The entites to add</param>
    Task AddRangeAsync(IEnumerable<TEntity> entities);

    /// <summary>
    /// Synchronously adds new entities (records)
    /// </summary>
    /// <param name="entities">The entites to add</param>
    void AddRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Asynchronously updates existing entity record
    /// </summary>
    /// <param name="entity">The entity/record to update</param>
    Task UpdateAsync(TEntity entity);

    /// <summary>
    /// Synchronously updates existing entity record
    /// </summary>
    /// <param name="entity">The entity/record to update</param>
    void Update(TEntity entity);

    /// <summary>
    /// Updates multiple entities
    /// </summary>
    /// <param name="entities">The list of entities/records to update</param>
    void UpdateRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Updates multiple entities
    /// </summary>
    /// <param name="entities">The list of entities/records to update</param>
    Task UpdateRangeAsync(IEnumerable<TEntity> entities);

    /// <summary>
    /// Asynchronously deletes entity record
    /// </summary>
    /// <param name="id">The entity's id</param>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Asynchronously deletes entity record
    /// </summary>
    /// <param name="entity">The entity's record</param>
    Task DeleteAsync(TEntity entity);

    /// <summary>
    /// Synchronously deletes entity record
    /// </summary>
    /// <param name="id">The entity's id</param>
    void Delete(Guid id);

    /// <summary>
    /// Synchronously deletes entity record
    /// </summary>
    /// <param name="entity">The entity's record</param>
    void Delete(TEntity entity);
}
