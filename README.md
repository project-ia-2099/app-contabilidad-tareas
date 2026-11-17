# Mis tareas de Contabilidad

Crea, edita, completa, reabre, elimina y filtra tus tareas por estado. Cada navegador guarda la clave de su lista individual. Usa el mismo navegador y perfil para volver a verla. Borrar los datos del sitio elimina la clave local de acceso.

## Desarrollo local

1. Usa .NET 10, Node.js 26 y un SQL Server accesible mediante la variable de entorno `ConnectionStrings__DefaultConnection`. La API aplica su migración al arrancar. El esquema de la aplicación y su historial de migraciones son `contabilidad_tareas`.
2. Inicia la API en una consola desde la raíz del proyecto.

   ```powershell
   dotnet run --project backend/Tareas.Api -- --urls http://localhost:8080
   ```

3. Inicia la interfaz en otra consola y abre la dirección que informa Vite.

   ```powershell
   cd frontend
   npm.cmd ci
   $env:VITE_API_URL = 'http://localhost:8080'
   npm.cmd run dev
   ```

Con Docker Compose, configura `MSSQL_SA_PASSWORD` y `TAREAS_COMPOSE_CONNECTION` en el entorno. La cadena debe apuntar al servicio `sqlserver` y usar las credenciales de esa instancia local. Ejecuta `docker compose up --build` y abre `http://localhost:8081`.

## Pruebas

```powershell
dotnet test backend/Tareas.sln
cd frontend
npm.cmd ci
npm.cmd test
npm.cmd run build
```

## API y despliegue

- `GET /health` comprueba la conexión a SQL Server.
- `GET /api/tasks` lista las tareas y devuelve los tres totales. Acepta `completed=true` o `completed=false`.
- `POST /api/tasks` crea una tarea con `title`.
- `PATCH /api/tasks/{id}` actualiza `title` o `completed`.
- `DELETE /api/tasks/{id}` elimina una tarea.

Las rutas de tareas requieren el encabezado `X-Task-Board` con la clave de la lista. GitHub Actions compila y prueba los pull requests. Al fusionar en `main`, despliega en Cloud Run QA y verifica la web y el ciclo completo de una tarea temporal. Producción requiere la aprobación del revisor de TI y ejecuta la misma verificación antes de publicar el enlace en el resumen de la ejecución.
