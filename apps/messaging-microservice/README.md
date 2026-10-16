# Messaging microservice

The Messaging Service of Bizcord. It accepts messages, stores them and announces them to the rest of the platform. See [`docs/`](../../docs/README.md) for the design.

## Layout

```
apps/messaging-microservice/
├── src/          ASP.NET Core Web API (Messaging.csproj, assembly Bizcord.Messaging)
├── tests/        xUnit tests (Messaging.Tests.csproj)
├── Dockerfile
└── README.md
```

## Run locally

Requires the .NET 8 SDK.

```bash
dotnet run --project apps/messaging-microservice/src
```

The service reads `RABBITMQ_HOST` (default `localhost`) to find RabbitMQ. The connection is made on first use, so the API starts without a broker.

The API listens on `http://localhost:5001`. Swagger UI is at `/swagger` in the Development environment, and `GET /health` returns `200`.

## Run with Docker

From the repository root (the Dockerfile needs the root as build context, because the service references `packages/`):

```bash
docker compose -f apps/messaging-microservice/docker-compose.yml up --build
```

| What | Where |
|---|---|
| Messaging API | http://localhost:5001 (`/health`, `/swagger`) |
| RabbitMQ management UI | http://localhost:15672 (user `guest`, password `guest`) |
| RabbitMQ AMQP | `localhost:5672` |

How it works:
- **Dockerfile**: multi-stage (SDK image to build, ASP.NET runtime image to run). Project files are copied and restored first, so the restore layer is cached until a dependency changes. The container runs as a non-root user and listens on port 8080 (the .NET 8 default).
- **docker-compose.yml**: `rabbitmq:3-management` with a health check (`rabbitmq-diagnostics check_port_connectivity`). The service uses `depends_on: condition: service_healthy`, so it only starts once the broker accepts connections. `depends_on` alone would only wait for the container to start.
- The service finds the broker through the `RABBITMQ_HOST=rabbitmq` environment variable. `rabbitmq` is the Compose service name, which Docker resolves to the container's IP.
- The root `.dockerignore` keeps `docs/`, `study/`, `book/`, `bin/` and `obj/` out of the build context.

Stop it with `docker compose -f apps/messaging-microservice/docker-compose.yml down`.

## Test

```bash
dotnet test Bizcord.sln
```

## Status

| Step | State |
|---|---|
| Project structure and boilerplate | Done |
| Message client (`packages/MessageClient`), registered in `Program.cs` | Done |
| Dockerfile and Compose | Done |
| `POST /api/messages` and tests | Not yet |
