# Messaging microservice

The Messaging Service of Bizcord. It accepts messages, stores them and publishes a `MessagePostedEvent` to RabbitMQ so other services can react. See [`docs/`](../../docs/README.md) for the design.

## Prerequisites

| Tool | Needed for |
|---|---|
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) (running) | Running the service with RabbitMQ, and the broker integration tests |
| [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) | Running the tests |

Free ports: `5001` (API), `5672` and `15672` (RabbitMQ).

The commands work in PowerShell and in bash (Git Bash, macOS, Linux, WSL).

**All commands below are run from the repository root** (the folder with `Bizcord.sln`), not from this folder.

## 1. Start it

```bash
docker compose -f apps/messaging-microservice/docker-compose.yml up --build
```

The first build downloads the .NET and RabbitMQ images and takes a few minutes. It is ready when the log shows `Now listening on: http://[::]:8080`.

| What | URL |
|---|---|
| Swagger UI | http://localhost:5001/swagger |
| Health check | http://localhost:5001/health (answers `Healthy`) |
| RabbitMQ management UI | http://localhost:15672 (user `guest`, password `guest`) |

Stop it with `Ctrl+C`, then `docker compose -f apps/messaging-microservice/docker-compose.yml down`.

## 2. Try it in Swagger

1. Open http://localhost:5001/swagger.
2. Open **POST /api/messages**, click **Try it out**, and use this body (or keep the generated example, which is also valid):

   ```json
   {
     "channelId": "11111111-1111-1111-1111-111111111111",
     "authorId": "22222222-2222-2222-2222-222222222222",
     "content": "Hello Bizcord"
   }
   ```

3. Click **Execute**. Expected: **201** with the saved message, including its `messageId`.
4. Copy the `messageId`, open **GET /api/messages/{id}**, paste it and click **Execute**. Expected: **200** with the same message.
5. Send the POST again with `"content": ""`. Expected: **400** with `"detail": "A message cannot be empty."`. Nothing is saved or published.

The same with curl (bash):

```bash
curl -i -X POST http://localhost:5001/api/messages \
  -H "Content-Type: application/json" \
  -d '{"channelId":"11111111-1111-1111-1111-111111111111","authorId":"22222222-2222-2222-2222-222222222222","content":"Hello Bizcord"}'
```

## 3. See the event in RabbitMQ

The service publishes to a RabbitMQ exchange. No other Bizcord service runs here, so nothing consumes the event yet. To look at it, give the exchange a queue in the management UI:

1. Post at least one message (step 2). This creates the exchange.
2. Open http://localhost:15672 and log in with `guest` / `guest`.
3. **Exchanges** tab: the exchange `Shared.Contracts.Events.MessagePostedEvent, Shared.Contracts` is listed, and its message rate goes up for every post.
4. **Queues and Streams** tab, **Add a new queue**: name it `demo-monitor` and click **Add queue**.
5. **Exchanges** tab: open the `MessagePostedEvent` exchange. Under **Bindings**, **To queue** `demo-monitor`, routing key `#`, then click **Bind**.
6. Post another message in Swagger.
7. **Queues and Streams** tab: open `demo-monitor`, then **Get messages** and **Get Message(s)**. The payload is the event, for example:

   ```json
   {"MessageId":"0be33f3e-...","ChannelId":"11111111-...","AuthorId":"22222222-...","Content":"Hello Bizcord","PostedAt":"2026-10-20T16:40:31.23Z"}
   ```

## 4. Run the tests

```bash
dotnet test Bizcord.sln
```

This runs every test in the repository: 38 here and 11 for `packages/MessageClient`. The 3 broker tests start a temporary RabbitMQ container themselves through [Testcontainers](https://dotnet.testcontainers.org/), so Docker must be running. The stack from step 1 does not need to be running, and it doesn't get in the way if it is.

Without Docker, skip the broker tests:

```bash
dotnet test Bizcord.sln --filter 'Category!=Broker'
```

| Test class (in `tests/`) | Level | What is real | What is faked |
|---|---|---|---|
| `Domain/MessageTests` | Unit | `Message` | nothing |
| `Controllers/MessagesControllerTests` | Unit with Moq (mocks) | the controller | repository, message client, channel directory |
| `Api/PostMessageTests` | Provider contract and HTTP round trip | the whole service | the broker (`FakeMessageClient`) |
| `Service/ChannelCheckServiceTests` | Service test | the whole service | the Channel service (WireMock stub) |
| `Integration/RealBrokerTests` | Integration | the whole service **and RabbitMQ** | nothing |

`Api/PostMessageTests.PostMessage_PublishedEvent_MeetsContract` is the most important test. It checks the **values** of every field in the published `MessagePostedEvent`, which protects the services that consume it.

## API

| Request | Result |
|---|---|
| `POST /api/messages` with `{"channelId": guid, "authorId": guid, "content": "text"}` | `201 Created` with the message and a `Location` header, and a `MessagePostedEvent` is published. `400` if the message is invalid (empty content, content over 4000 characters, empty ids). `503` if the message was saved but RabbitMQ could not be reached. |
| `GET /api/messages/{id}` | `200` with the message, or `404` |
| `GET /health` | `200 Healthy` |

Messages are kept in memory, so they are gone when the service restarts.