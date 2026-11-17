namespace Tareas.Domain;

public interface ITaskRepository
{
    Task<List<TaskItem>> ListAsync(Guid boardId, CancellationToken ct);
    Task<TaskItem?> FindAsync(Guid boardId, Guid id, CancellationToken ct);
    void Add(TaskItem item);
    void Remove(TaskItem item);
    Task SaveAsync(CancellationToken ct);
}
