---
trigger: always_on
description: Reglas obligatorias de Golden Path para flujo Git, Conventional Commits, Clean Architecture, seguridad y MEMORY.md.
---

# Reglas Inquebrantables del Proyecto (Golden Path)

1. **`MEMORY.md` es la Fuente de Verdad:** Lee `MEMORY.md` al iniciar. Todo plan aprobado por el usuario debe escribirse en `MEMORY.md` antes de ejecutar. Actualiza `MEMORY.md` y `README.md` al concluir cada tarea.
2. **Secuencialidad:** No avances a una tarea nueva sin haber terminado y verificado la tarea actual.
3. **Ramas y Conventional Commits v1.0.0:**
   - Prohibido hacer commits directos sobre `main`. Crea siempre una rama (`feat/...`, `fix/...`, `chore/...`, `docs/...`, `refactor/...`).
   - Usa siempre mensajes de commit bajo el estándar **Conventional Commits v1.0.0** (`<tipo>(<alcance>): <descripción>`).
4. **Pull Requests con `gh` y No Auto-Merge:**
   - Todo cambio se sube al remoto y se abre un Pull Request con `gh pr create`.
   - **Prohibido fusionar (`merge`) a `main`** a menos que el usuario lo indique explícitamente.
5. **Cero Archivos Basura y Cero Datos Sensibles:**
   - No subas archivos ajenos al core del proyecto (ante la duda, pregunta al usuario).
   - Prohibido subir credenciales, cadenas de conexión o datos sensibles. Usa **GCP Secret Manager** en nube y `dotnet user-secrets` en local.
6. **Código DRY y Cero Fallbacks Silenciosos:**
   - Reutiliza código (DRY).
   - **No crees fallbacks silenciosos** (`try/catch` que traguen errores o valores por defecto inventados). Si tienes una duda técnica o ambigüedad, pregunta al usuario; no asumas.
7. **Stack y Clean Architecture:**
   - Backend en `.NET 10` Minimal APIs dividido en `Domain`, `Application`, `Infrastructure`, `Api` y `Tests` (`xUnit`).
   - Frontend en React + TypeScript con **Node.js 26 LTS** y `Vitest`.
8. **Convención de Nombres de Repositorio (`app-<area>-<nombre>`):**
   - Todo proyecto nuevo debe nombrarse siguiendo el formato `app-<area>-<nombre>` en minúsculas (`kebab-case`, máximo 35 caracteres, regex `^app-[a-z0-9]+-[a-z0-9-]+$`, sin guiones bajos ni mayúsculas) para compatibilidad directa con **Google Cloud Run**.
9. **Aislamiento en Base de Datos Compartida (Esquemas SQL + Migraciones):**
   - Todas las aplicaciones comparten la misma base de datos por entorno (`VicoderLocalDb`, `BooksDb_QA`, `BooksDb_Prod`), pero cada aplicación **debe aislar sus tablas en su propio esquema SQL** (`<area>_<nombre>`).
   - Configura `modelBuilder.HasDefaultSchema("<esquema>")` y `MigrationsHistoryTable("__EFMigrationsHistory", "<esquema>")` en `AppDbContext`, aplicando migraciones con `Database.Migrate()` (solo si `Database.IsRelational()`). **Prohibido usar `EnsureCreated()` o escribir tablas en `dbo`.**
10. **Lógica de Negocio Exclusiva del Backend:**
    - Cálculos, reglas de negocio y validaciones críticas residen únicamente en `Domain` y `Application` (listo para futura unificación por microservicios). El Frontend es solo presentación.
11. **UI Minimalista Pastel, Font Awesome Free y Cero Emojis:**
    - Prohibido usar emoticones/emojis Unicode en botones, títulos, tablas o alertas. Usa únicamente **Font Awesome Free** (`@fortawesome/fontawesome-free`) y la paleta **Minimalista Pastel** definida en `scaffold-clean-fullstack`.
