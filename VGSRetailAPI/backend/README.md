# Backend Structure

ASP.NET Core modular monolith workspace root.

Key directories:
- `src/ApiHost`: API hosting boundary
- `src/Shared`: cross-cutting shared concerns
- `src/Modules`: feature modules with clear boundaries
- `src/Infrastructure`: infrastructure adapters
- `src/Workers`: background processing boundaries
- `tests`: backend test layers

