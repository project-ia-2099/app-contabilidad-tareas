namespace Tareas.Domain;

public sealed class TaskItem(Guid boardId, string title)
{
    public const int TitleLimit = 160;

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid BoardId { get; private set; } = boardId;
    public string Title { get; private set; } = title;
    public bool Completed { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public void Rename(string title) => Title = title;
    public void SetCompleted(bool completed) => Completed = completed;
}
