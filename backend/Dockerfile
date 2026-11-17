FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Tareas.Domain/Tareas.Domain.csproj Tareas.Domain/
COPY Tareas.Application/Tareas.Application.csproj Tareas.Application/
COPY Tareas.Infrastructure/Tareas.Infrastructure.csproj Tareas.Infrastructure/
COPY Tareas.Api/Tareas.Api.csproj Tareas.Api/
RUN dotnet restore Tareas.Api/Tareas.Api.csproj
COPY . .
RUN dotnet publish Tareas.Api/Tareas.Api.csproj -c Release --no-restore -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Tareas.Api.dll"]
