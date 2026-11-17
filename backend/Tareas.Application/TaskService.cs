using Tareas.Domain;

namespace Tareas.Application;

public record TaskView(Guid Id, string Title, bool Completed);
public record TaskList(TaskView[] Items, int Total, int Pending, int Completed);

public sealed class TaskService(ITaskRepository repository)
{
    public static bool TryNormalizeTitle(string? input, out string title)
    {
        title = input?.Trim() ?? "";
        return title.Length is > 0 and <= TaskItem.TitleLimit;
    }

    public async Task<TaskList> ListAsync(Guid boardId, bool? completed, CancellationToken ct)
    {
        var tasks = await repository.ListAsync(boardId, ct);
        var done = tasks.Count(task => task.Completed);
        var items = tasks.Where(task => completed is null || task.Completed == completed)
            .Select(ToView).ToArray();

        return new TaskList(items, tasks.Count, tasks.Count - done, done);
    }

    public async Task<TaskView> CreateAsync(Guid boardId, string title, CancellationToken ct)
    {
        var item = new TaskItem(boardId, title);
        repository.Add(item);
        await repository.SaveAsync(ct);
        return ToView(item);
    }

    public async Task<TaskView?> UpdateAsync(Guid boardId, Guid id, string? title, bool? completed, CancellationToken ct)
    {
        var item = await repository.FindAsync(boardId, id, ct);
        if (item is null)
            return null;

        if (title is not null)
            item.Rename(title);
        if (completed.HasValue)
            item.SetCompleted(completed.Value);

        await repository.SaveAsync(ct);
        return ToView(item);
    }

    public async Task<bool> DeleteAsync(Guid boardId, Guid id, CancellationToken ct)
    {
        var item = await repository.FindAsync(boardId, id, ct);
        if (item is null)
            return false;

        repository.Remove(item);
        await repository.SaveAsync(ct);
        return true;
    }

    private static TaskView ToView(TaskItem item) => new(item.Id, item.Title, item.Completed);
}
