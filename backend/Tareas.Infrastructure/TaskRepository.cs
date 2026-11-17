using Microsoft.EntityFrameworkCore;
using Tareas.Domain;

namespace Tareas.Infrastructure;

internal sealed class TaskRepository(AppDbContext db) : ITaskRepository
{
    public Task<List<TaskItem>> ListAsync(Guid boardId, CancellationToken ct) =>
        db.Tasks.AsNoTracking().Where(item => item.BoardId == boardId)
            .OrderByDescending(item => item.CreatedAt).ThenBy(item => item.Id).ToListAsync(ct);

    public Task<TaskItem?> FindAsync(Guid boardId, Guid id, CancellationToken ct) =>
        db.Tasks.SingleOrDefaultAsync(item => item.BoardId == boardId && item.Id == id, ct);

    public void Add(TaskItem item) => db.Tasks.Add(item);

    public void Remove(TaskItem item) => db.Tasks.Remove(item);

    public async Task SaveAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
}
