using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tareas.Application;
using Tareas.Domain;
using Tareas.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();
builder.Services.AddTaskStorage();
builder.Services.AddScoped<TaskService>();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
{
    // The browser supplies an explicit capability header. No ambient credentials are allowed.
    policy.AllowAnyOrigin().AllowAnyHeader().WithMethods("GET", "POST", "PATCH", "DELETE");
}));

var app = builder.Build();
app.UseExceptionHandler();
app.UseCors();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsRelational())
        await db.Database.MigrateAsync();
}

app.MapGet("/health", async (AppDbContext db, CancellationToken ct) =>
    await db.Database.CanConnectAsync(ct) ? Results.Ok(new { status = "ok" }) : Results.StatusCode(503));

var tasks = app.MapGroup("/api/tasks");
tasks.AddEndpointFilter(async (context, next) =>
{
    context.HttpContext.Response.Headers.CacheControl = "no-store";
    var board = context.GetArgument<Guid>(0);
    return board == Guid.Empty
        ? Results.BadRequest(new { detail = "La clave de la lista no es válida." })
        : await next(context);
});

tasks.MapGet("", ([FromHeader(Name = "X-Task-Board")] Guid board, bool? completed,
    TaskService service, CancellationToken ct) => service.ListAsync(board, completed, ct));

tasks.MapPost("", async ([FromHeader(Name = "X-Task-Board")] Guid board, CreateTask request,
    TaskService service, CancellationToken ct) =>
{
    if (!TaskService.TryNormalizeTitle(request.Title, out var title))
        return InvalidTitle();

    var item = await service.CreateAsync(board, title, ct);
    return Results.Json(item, statusCode: StatusCodes.Status201Created);
});

tasks.MapPatch("/{id:guid}", async ([FromHeader(Name = "X-Task-Board")] Guid board, Guid id,
    UpdateTask request, TaskService service, CancellationToken ct) =>
{
    if (request.Title is null && request.Completed is null)
        return Results.BadRequest(new { detail = "Indica el cambio que quieres guardar." });

    string? title = null;
    if (request.Title is not null && !TaskService.TryNormalizeTitle(request.Title, out title))
        return InvalidTitle();

    var item = await service.UpdateAsync(board, id, title, request.Completed, ct);
    return item is null ? Results.NotFound() : Results.Ok(item);
});

tasks.MapDelete("/{id:guid}", async ([FromHeader(Name = "X-Task-Board")] Guid board, Guid id,
    TaskService service, CancellationToken ct) =>
        await service.DeleteAsync(board, id, ct) ? Results.NoContent() : Results.NotFound());

app.Run();

static IResult InvalidTitle() => Results.BadRequest(new
{
    detail = $"Escribe un título de 1 a {TaskItem.TitleLimit} caracteres."
});

record CreateTask(string? Title);
record UpdateTask(string? Title, bool? Completed);
public partial class Program;
