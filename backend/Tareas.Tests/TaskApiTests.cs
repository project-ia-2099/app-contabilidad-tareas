using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tareas.Application;
using Tareas.Infrastructure;

namespace Tareas.Tests;

public sealed class TaskApiTests
{
    [Fact]
    public async Task TasksCanBeManagedAndAreIsolatedByBoard()
    {
        using var host = CreateHost();
        using var client = host.CreateClient();
        using var other = host.CreateClient();
        client.DefaultRequestHeaders.Add("X-Task-Board", Guid.NewGuid().ToString());
        other.DefaultRequestHeaders.Add("X-Task-Board", Guid.NewGuid().ToString());

        Assert.Equal(HttpStatusCode.BadRequest,
            (await client.PostAsJsonAsync("/api/tasks", new { title = "   " })).StatusCode);
        using var created = await client.PostAsJsonAsync("/api/tasks", new { title = " Conciliar " });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var task = (await created.Content.ReadFromJsonAsync<TaskView>())!;
        Assert.Equal("Conciliar", task.Title);

        var path = $"/api/tasks/{task.Id}";
        Assert.Empty((await other.GetFromJsonAsync<TaskList>("/api/tasks"))!.Items);
        Assert.Equal(HttpStatusCode.NotFound, (await other.DeleteAsync(path)).StatusCode);

        using var updated = await client.PatchAsJsonAsync(path, new { title = "Conciliar el mes", completed = true });
        updated.EnsureSuccessStatusCode();
        var finished = (await client.GetFromJsonAsync<TaskList>("/api/tasks?completed=true"))!;
        Assert.Equal("Conciliar el mes", Assert.Single(finished.Items).Title);
        Assert.Equal(0, finished.Pending);
        Assert.Empty((await client.GetFromJsonAsync<TaskList>("/api/tasks?completed=false"))!.Items);

        using var reopened = await client.PatchAsJsonAsync(path, new { completed = false });
        Assert.False((await reopened.Content.ReadFromJsonAsync<TaskView>())!.Completed);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync(path)).StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<TaskList>("/api/tasks"))!.Items);
    }

    private static WebApplicationFactory<Program> CreateHost()
    {
        var database = Guid.NewGuid().ToString();
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
                services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(database));
            }));
    }
}
