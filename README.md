# Bizcord: Messaging microservice

Backend of **Bizcord**, a Discord-like enterprise communication platform, built as a monorepo for the System Integration course (E2026). This repository contains one microservice, the **Messaging Service**, plus the shared packages it needs.

| Folder | Contents |
|---|---|
| [`docs/`](docs/README.md) | Analysis and design: domains, boundaries, data ownership, interactions, class diagram, work plan |
| `apps/messaging-microservice/` | The Messaging Service (`src/`, `tests/`, `Dockerfile`, `docker-compose.yml`) |
| `packages/MessageClient/` | `IMessageClient` abstraction over RabbitMQ (EasyNetQ) |
| `packages/Shared.Contracts/` | Events shared between services |

Start with [`docs/README.md`](docs/README.md) and [`docs/06-work-plan.md`](docs/06-work-plan.md) to find your way around. Each course week was developed on its own branch and merged into `main`.

## Quick start

```bash
# run the service and RabbitMQ
docker compose -f apps/messaging-microservice/docker-compose.yml up --build
# API: http://localhost:5001/swagger   RabbitMQ UI: http://localhost:15672 (guest/guest)

# run all tests (needs the .NET 8 SDK; the broker tests also need Docker)
dotnet test Bizcord.sln
```
