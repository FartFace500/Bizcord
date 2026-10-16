# Bizcord: Messaging microservice

Backend of **Bizcord**, a Discord-like enterprise communication platform, built as a monorepo for the System Integration course (E2026). This repository contains one microservice, the **Messaging Service**, plus the shared packages it needs.

| Folder | Contents |
|---|---|
| [`docs/`](docs/README.md) | Analysis and design: domains, boundaries, data ownership, interactions, class diagram, work plan |
| `apps/messaging-microservice/` | The Messaging Service (`src/`, `tests/`, `Dockerfile`, `docker-compose.yml`) |
| `packages/MessageClient/` | `IMessageClient` abstraction over RabbitMQ (EasyNetQ) |
| `packages/Shared.Contracts/` | Events shared between services |
| `study/` | Chapter summaries and small study examples (not part of the assignment) |

Start with [`docs/README.md`](docs/README.md) and [`docs/07-work-plan.md`](docs/07-work-plan.md) to find your way around. The code folders appear branch by branch, as described there.
