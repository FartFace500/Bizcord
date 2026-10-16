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

## API

| Request | Result |
|---|---|
| `POST /api/messages` with `{"channelId": guid, "authorId": guid, "content": "text"}` | `201 Created` with the message and a `Location` header, and a `MessagePostedEvent` is published. `400` if the message is invalid (empty content, content over 4000 characters, empty ids). |
| `GET /api/messages/{id}` | `200` with the message, or `404` |
| `GET /health` | `200 Healthy` |

Optional channel check: if `ChannelService__BaseUrl` is set (for example `http://channels:8080`), the service calls `GET {baseUrl}/api/channels/{channelId}` before saving. `404` from that service gives `404`, any other failure gives `503`. When it is not set, any channel id is accepted.

Try it (with the Docker stack running):

```bash
curl -i -X POST http://localhost:5001/api/messages \
  -H 'Content-Type: application/json' \
  -d '{"channelId":"11111111-1111-1111-1111-111111111111","authorId":"22222222-2222-2222-2222-222222222222","content":"Hello"}'
```

## Tests

```bash
dotnet test apps/messaging-microservice/tests                       # everything (the broker tests start RabbitMQ with Docker)
dotnet test apps/messaging-microservice/tests --filter "Category!=Broker"   # fast tests, no Docker needed
dotnet test apps/messaging-microservice/tests --filter "Category=Broker"    # only the real-RabbitMQ tests
```

| Test class | Level | What is real | What is faked |
|---|---|---|---|
| `Domain/MessageTests` | Unit | `Message` | nothing |
| `Controllers/MessagesControllerTests` | Unit with Moq (mocks) | the controller | repository, message client, channel directory |
| `Api/PostMessageTests` | Provider contract and HTTP round trip | the whole service | the broker (`FakeMessageClient`) |
| `Service/ChannelCheckServiceTests` | Service test | the whole service | the Channel service (WireMock stub) |
| `Integration/RealBrokerTests` | Integration | the whole service **and RabbitMQ** | nothing |

`Api/PostMessageTests.PostMessage_PublishedEvent_MeetsContract` is the most important test: it checks the **values** of every field in the published `MessagePostedEvent`, which protects the services that consume it.

The broker tests use [Testcontainers](https://dotnet.testcontainers.org/) to start a throwaway RabbitMQ (needs Docker). To use an existing broker instead, set `RABBITMQ_HOST`.

## Test (all projects)

```bash
dotnet test Bizcord.sln
```

## Status

| Step | State |
|---|---|
| Project structure and boilerplate | Done |
| Message client (`packages/MessageClient`), registered in `Program.cs` | Done |
| Dockerfile and Compose | Done |
| `POST /api/messages`, `GET /api/messages/{id}` and tests | Done |
