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

## Test

```bash
dotnet test Bizcord.sln
```

## Status

| Step | State |
|---|---|
| Project structure and boilerplate | Done |
| Message client (`packages/MessageClient`), registered in `Program.cs` | Done |
| Dockerfile and Compose | Not yet |
| `POST /api/messages` and tests | Not yet |
