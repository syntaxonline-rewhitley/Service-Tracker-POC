# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Service Tracker POC — a field-service ticketing app with a .NET 8 Web API backend and a React/Vite frontend, backed by PostgreSQL. All source lives under `src/`.

- `src/ServiceTracker.Api` — ASP.NET Core 8 Web API
- `src/ServiceTracker.Api.Tests` — xUnit test project for the API
- `src/ServiceTracker.Web` — React + TypeScript + Vite + Tailwind frontend
- `deployment/ansible` — Ansible playbook used by CI to deploy the Docker Compose stack to a VPS

## Commands

### Backend (.NET)

Run from `src/`:

```bash
dotnet restore "Service Tracker POC.sln"
dotnet build "Service Tracker POC.sln"
dotnet test "Service Tracker POC.sln"                     # all tests
dotnet test --filter FullyQualifiedName~ServiceTicketsControllerTests  # single class
dotnet test --filter "FullyQualifiedName~ServiceTicketsControllerTests.GetAll_ReturnsOkWithMappedTickets" # single test
dotnet run --project ServiceTracker.Api                   # run API locally (see launchSettings.json for ports)
```

EF Core migrations (run from `src/ServiceTracker.Api`):

```bash
dotnet ef migrations add <Name> -o Data/Migrations
dotnet ef database update
```

### Frontend (React/Vite)

Run from `src/ServiceTracker.Web`:

```bash
npm install
npm run dev        # Vite dev server on :5173, proxies /api -> http://localhost:8080
npm run build      # tsc typecheck + vite build
npm run preview
```

There is no configured lint/test script for the frontend; `npm run build` (which runs `tsc`) is the primary correctness check.

### Full stack via Docker Compose

Run from `src/`:

```bash
docker compose -f docker-compose.yml -f docker-compose.override.yml -f docker-compose.frontend.yml up --build
```

- API: http://localhost:5092 (override maps container port 8080 → host 5092)
- Web UI: http://localhost:3000
- pgAdmin: http://localhost:5050
- RabbitMQ management: http://localhost:15672

Requires `POSTGRES_USER`/`POSTGRES_PASSWORD`, `RABBITMQ_USER`/`RABBITMQ_PASSWORD`, `PGADMIN_EMAIL`/`PGADMIN_PASSWORD` env vars (see `docker-compose.yml`). Note RabbitMQ is provisioned in Compose but not currently wired into `Program.cs` — no messaging code consumes it yet.

## Architecture

### Backend layering

Controllers → Repositories (interface + implementation per entity) → `ServiceTrackerDbContext` (EF Core + Npgsql). Each entity (`Company`, `Contact`, `Technician`, `ServiceTicket`) has a matching `I<Entity>Repository` / `<Entity>Repository` pair registered as scoped services in `Program.cs`. Controllers map entities to response DTOs defined in `Models/` (e.g. `ServiceTicketModels.cs`) via a private `ToResponse` method — controllers never return EF entities directly.

Repositories that return related data use an `IQueryable<T> WithIncludes()` helper to eagerly load navigation properties (`Company`, `Contact`, `Technician`), and use `AsNoTracking()` for read paths.

### Auth model

- ASP.NET Core Identity (`IdentityUser`/`IdentityRole`) + JWT bearer auth. `RoleClaimType` is explicitly set to `"role"` and `MapInboundClaims = false` — this is required because .NET 8's `JsonWebTokenHandler` doesn't apply `JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear()` on its own. Don't remove these settings when touching auth; role-based `[Authorize(Roles = "...")]` checks will silently stop matching if you do.
- Three roles: `Admin`, `Dispatcher`, `Technician`. Roles and a default account per role are seeded on startup in `Program.cs` (configurable via `DefaultAdmin:*`, `DefaultDispatcher:*`, `DefaultTechnician:*` config keys).
- A `Technician` role account is linked to a `Technician` entity record via `TechnicianRepository.LinkUserAsync` — the technician-specific endpoints (`GET /api/servicetickets/my`) resolve the current user's `Technician` row from the JWT's `NameIdentifier`/`sub` claim, not from a `TechnicianId` route param.
- Migrations run automatically at startup (`db.Database.MigrateAsync()`) — there is no separate migration-apply step in deployment.

### Frontend structure

- Routing (`App.tsx`) is fully role-gated using `<RequireAuth roles={[...]} />` route wrappers: unauthenticated → `/login`, authenticated-but-wrong-role → `/unauthorized`. Route trees are grouped by role prefix (`/admin/*`, `/dispatcher/*`, `/technician/*`).
- `AuthContext` decodes the JWT client-side (`jwt-decode`) to derive `email`/`roles`/expiry and auto-logs-out via `setTimeout` when the token expires — there is no refresh-token flow.
- All API calls go through `src/lib/api.ts`, a single Axios instance with a request interceptor that attaches `Authorization: Bearer <token>` from `localStorage`. Add new endpoints there rather than calling `axios` directly from components.
- API base URL resolution: `window._env_?.SERVICETRACK_API_BASE_URL` (runtime-injected) falls back to `/api` (proxied by Vite in dev). In Docker builds, `VITE_API_URL` is baked into the JS bundle at build time via `--build-arg` (see `docker-compose.frontend.yml` and the deployment workflow).
- Pages are organized by role under `src/pages/{admin,dispatcher,technician}/`; shared/public pages sit directly in `src/pages/`.

### Deployment

CI/CD is a single manually-triggered workflow (`.github/workflows/deployment.yml`, `workflow_dispatch`): runs `.NET` tests → builds and pushes `servicetrackerapi` and `servicetrackerweb` images to `ghcr.io` → SSHes into a VPS and runs `ansible-deploy servicetrack/deploy.yml`, which templates env files and a Compose spec from `deployment/ansible/group_vars/*` and brings the stack up with `community.docker.docker_compose_v2` (pull-only, `build: never` — images must already be pushed).
