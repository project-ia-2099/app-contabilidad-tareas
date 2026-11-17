using System.Reflection;
using Tareas.Application;
using Tareas.Domain;

namespace Tareas.Tests;

public sealed class ArchitectureTests
{
    [Fact]
    public void BusinessLayersOnlyUseAllowedProjectReferences()
    {
        Assert.Empty(ProjectReferences(typeof(TaskItem).Assembly));
        Assert.Equal(new[] { "Tareas.Domain" }, ProjectReferences(typeof(TaskService).Assembly));
    }

    private static IEnumerable<string?> ProjectReferences(Assembly assembly) =>
        assembly.GetReferencedAssemblies().Select(reference => reference.Name)
            .Where(name => name?.StartsWith("Tareas.", StringComparison.Ordinal) == true);
}
