namespace Moveo_backend.Shared.Domain.Repositories;

public interface IUnitOfWork
{
    /// <summary>
    ///     Save changes to the repository
    /// </summary>
    /// <returns></returns>
    Task CompleteAsync();
}