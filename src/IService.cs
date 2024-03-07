namespace LocalServer;

public interface IService
{
    Task<string?> GetAll(string entity, CancellationToken cancellationToken);
    Task<string?> GetById(string entity, string id, CancellationToken cancellationToken);
    Task<string?> Create(string entity, string newData, CancellationToken cancellationToken);
    Task<bool> Generate(string entity, int count, string template, CancellationToken cancellationToken);
    Task<bool> Update(string entity, string id, string newData, CancellationToken cancellationToken);
    Task<bool> Delete(string entity, string id, CancellationToken cancellationToken);
    Task<bool> DeleteAll(string entity, CancellationToken cancellationToken);
}