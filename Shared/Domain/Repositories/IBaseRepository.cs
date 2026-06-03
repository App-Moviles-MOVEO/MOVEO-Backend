namespace Moveo_backend.Shared.Domain.Repositories;

public interface IBaseRepository<TEntity>
{
    /// <summary>
    ///     Add an entity to the repository
    /// </summary>
    /// <param name="entity">
    ///     The entity to add
    /// </param>
    /// <returns></returns>
    Task AddAsync(TEntity entity);

    /// <summary>
    ///     Find an entity by its id
    /// </summary>
    /// <param name="id">
    ///     The id of the entity to find
    /// </param>
    /// <returns>
    ///     The entity if found, otherwise null
    /// </returns>
    Task<TEntity?> FindByIdAsync(int id);

    void Update(TEntity entity);

    /// <summary>
    ///     Remove an entity from the repository
    /// </summary>
    /// <param name="entity">
    ///     The entity to remove
    /// </param>
    void Remove(TEntity entity);

    /// <summary>
    ///     List all entities in the repository
    /// </summary>
    /// <returns>
    ///     A list of all entities in the repository
    /// </returns>
    Task<IEnumerable<TEntity>> ListAsync();
}