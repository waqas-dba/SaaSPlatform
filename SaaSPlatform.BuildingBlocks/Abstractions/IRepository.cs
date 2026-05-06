namespace SaaSPlatform.BuildingBlocks.Abstractions;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task AddAsync(T entity);
    IQueryable<T> Query();
}