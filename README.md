<div align="center">

# 🎬 MidIA — AI-Powered Media Recommendations

An ASP.NET Core 10.0 web app that chats with an LLM (`llama-3.3-70b-versatile` through Groq)
to suggest books, movies, games and other digital media — then lets you save the lists
you like and revisit them from a personal dashboard.

</div>

## ✨ Features

- 🔐 **ASP.NET Core Identity** — register / log in, account confirmation.
- 💬 **AI Chat page** — describe what you're in the mood for, get a curated list back.
- 💾 **Save lists** — persist the prompt + AI response to your account.
- 📚 **Personal dashboard** — see, open and delete your saved lists.
- 🐘 **PostgreSQL** via EF Core (Npgsql), with the `GeneratedList` payload stored as JSON.
- 🐳 **Dockerized** — multi-stage Dockerfile + Compose stack that brings up the app, Postgres,
  and an opt-in integration-test runner.
- 🧪 **xUnit integration tests** that boot the app via `WebApplicationFactory` and bypass
  auth with a test scheme.

## 🏗️ Tech stack

| Layer | Technology |
| --- | --- |
| Runtime | .NET 10.0 |
| Web | ASP.NET Core Razor Pages + Areas (Identity UI) |
| Auth | ASP.NET Core Identity + EF Core stores |
| ORM | Entity Framework Core 10.0.6 |
| Database | PostgreSQL (Npgsql 10.0.1) |
| AI | Groq API via `GroqApiLibrary` 2.0.0 — model `llama-3.3-70b-versatile` |
| Tests | xUnit 2.9.3, `Microsoft.AspNetCore.Mvc.Testing`, TestHost |

## 📁 Project structure

```
MidIAProjeto/
├── MidIAProjeto/                  # main ASP.NET Core web app
│   ├── Pages/                     # Razor Pages (Index, AiChat, Error, …)
│   ├── Areas/Identity/            # Identity UI (Login, Register, Dashboard, ListView)
│   ├── Data/                      # ApplicationDbContext, MediaList, ListViewModel
│   ├── Migrations/                # EF Core migrations
│   ├── Service/GroqClient.cs      # Wraps GroqApiClient; chat completion
│   ├── Program.cs
│   └── MidIAProjeto.csproj
├── MidIAProjeto.IntegrationTests/ # xUnit integration tests
│   ├── AIChatPageTests.cs
│   ├── DashboardPageTests.cs
│   ├── TestAuthHandler.cs         # Bypasses auth with a fake user
│   └── Utilities/HelperFunctions.cs
├── compose.yaml                   # app + Postgres + opt-in tests
├── Dockerfile                     # multi-stage build for the web app
├── Dockerfile.tests               # builds the solution, runs `dotnet test`
├── entrypoint.sh                  # applies EF Core migrations, then runs the app
├── efbundle                       # self-contained EF Core migrations bundle
├── dotnet-tools.json              # dotnet-ef local tool manifest
└── .env.example                   # template for secrets
```

## 🚀 Quick start (local dev, no Docker)

Prerequisites: .NET SDK 10.0, PostgreSQL 14+.

```bash
# 1. Clone & restore
git clone https://github.com/<your-user>/MidIAProjeto.git
cd MidIAProjeto
dotnet restore

# 2. Set up secrets (User Secrets keeps them out of source control)
dotnet user-secrets init   # already wired via UserSecretsId in the csproj
dotnet user-secrets set "Groq:ApiKey" "<your-groq-key>"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
    "Server=localhost;Port=5432;Database=MidIADataBase;Username=postgres;Password=<your-pw>"

# 3. Apply migrations (the project ships an efbundle in the repo root)
./efbundle --connection "<your-connection-string>"

# 4. Run
dotnet run --project MidIAProjeto
# then open http://localhost:5011 (check the launchSettings for the assigned port)
```

## 🐳 Quick start (Docker)

The Compose stack brings up Postgres and the app, applies the EF Core migrations
automatically on container start, and exposes the app on `http://localhost:8080`.

```bash
cp .env.example .env
#edit .env file and add your Groq Api Key in GROQ_API_KEY

docker compose up --build
```

`docker compose` waits for the Postgres healthcheck to pass before starting the app,
and the app's entrypoint runs `./efbundle` against the same connection string before
launching the web server.

## ⚙️ Configuration

All configuration is overridable via environment variables (or a `.env` file loaded by
Compose). Double-underscores (`__`) translate to nested config keys.

| Variable | Description | Default |
| --- | --- | --- |
| `ConnectionStrings__DefaultConnection` | Npgsql connection string | `Server=localhost;Port=5432;Database=MidIADataBase;User Id=<user>;` |
| `Groq__ApiKey` | Groq API key | *(required at runtime)* |
| `ASPNETCORE_ENVIRONMENT` | `Development` / `Production` / `Testing` | `Production` |
| `ASPNETCORE_URLS` | Bind address | `http://+:8080` |
| `APP_PORT` | Host port mapped to the container | `8080` |
| `POSTGRES_USER` / `POSTGRES_PASSWORD` / `POSTGRES_DB` | Used by the `db` service | `midia` / `midia` / `MidIADataBase` |

`Program.cs` automatically picks up the env-var form when
`DOTNET_RUNNING_IN_CONTAINER=true` is set (which the Dockerfile does).

## 🧪 Running the tests

The integration tests boot the full app via `WebApplicationFactory<Program>` and
override the authentication scheme with a `TestAuthHandler` that always logs in as
`admin-test-id-123 / admin@midia.com`. They still hit a real Postgres because the
seed helpers write through EF Core.

```bash
# Option A — on the host (against a Postgres you have running locally)
dotnet test MidIAProjeto.IntegrationTests

# Option B — inside Compose (no local Postgres needed)
docker compose --profile tests run --build --rm tests
```

The `tests` service is gated by the `tests` Compose profile, so it won't start with
the default `docker compose up`. The test project is provided to its build context
through a named BuildKit context, so it stays excluded from the main app's build
context.

## 🛠️ Migrations

The repo ships a self-contained **efbundle** (`./efbundle`, produced with
`dotnet ef migrations bundle --output ./efbundle --self-contained`). The
container's entrypoint runs it on every start with the same connection string as
the app — it's idempotent and exits quickly when there's nothing pending.

To regenerate the bundle after adding a migration:

```bash
dotnet ef migrations bundle \
  --project MidIAProjeto \
  --output ./efbundle \
  --self-contained \
  --target-runtime linux-x64
```


## 📦 Notable implementation details

- **JSON-stored recommendation lists.** `MediaList.GeneratedList` is a complex
  property mapped with `modelBuilder.Entity<MediaList>().ComplexProperty(ml =>
  ml.GeneratedList, d => d.ToJson())`, so the recommendation payload lands in a
  single JSON column in PostgreSQL.
- **Idempotent migrations on start.** `entrypoint.sh` runs `./efbundle
  --connection "$ConnectionStrings__DefaultConnection"` before exec'ing the app.
  Restarts are safe.
- **Cache-friendly Docker builds.** Each Dockerfile copies only the `.csproj`
  (and the test project, via a named context) before the full source, so NuGet
  restore is cached even when source files change.
- **Test authentication.** `TestAuthHandler` short-circuits the auth pipeline in
  tests, so handlers that require `[Authorize]` can be exercised without a
  real login flow.

## 📄 License

This project is licensed under the MIT License — see [LICENSE](LICENSE) for details.
