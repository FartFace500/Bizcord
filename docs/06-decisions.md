# 6. Design decisions

Short decision records. Each one states the choice, the alternatives, and the trade-off.

## D1. Messaging is the domain I implement

- **Choice:** Messaging Service, Track A of week 40.
- **Why:** it is the provider of the system's central event, so it gets the most meaningful provider contract tests, and it has real domain rules.
- **Trade-off:** every consumer depends on its contract, so the contract must be stable and tested.

## D2. One microservice, one monorepo

- **Choice:** `apps/` for deployables, `packages/` for shared libraries, `docs/` for design.
- **Why:** the course asks for a monorepo that could hold more services. The Dockerfile build context is the repository root so it can copy `packages/`.
- **Trade-off:** shared packages couple services at build time. We keep `Shared.Contracts` to event types only.

## D3. Events for fan-out, REST only for the user-facing call

- **Choice:** `POST /api/messages` is REST. Everything that happens after saving is an event.
- **Alternatives:** Messaging calls each consumer over REST. Rejected: Messaging would need to know every consumer, and one slow consumer would slow every post.
- **Trade-off:** eventual consistency, and failures in consumers are not visible to the sender.

## D4. `IMessageClient` hides RabbitMQ

- **Choice:** a small interface (`PublishAsync`, `SubscribeAsync`) with an EasyNetQ adapter. No EasyNetQ types in the interface.
- **Why:** assignment goal: swap RabbitMQ for another broker by writing one adapter. It also makes unit testing easy, because an interface can be mocked or faked without any special design (see the study notes on Moq and `virtual`).
- **Trade-off:** we expose only the common subset of what a broker can do (no routing keys, no headers) until a use case needs more.

## D5. In-memory storage first, behind `IMessageRepository`

- **Choice:** `InMemoryMessageRepository` (a thread-safe dictionary).
- **Why:** the assignment does not require a database yet, and the interface means one can be added later by writing a new class and changing one DI line.
- **Trade-off:** data is lost on restart, and only one instance can run. Both are fine for this stage.

## D6. Domain object protects its own rules

- **Choice:** `Message.Create` validates and throws `DomainException`. The constructor is private.
- **Why:** invalid messages cannot exist, regardless of which entry point creates them.

## D7. The channel check is optional and off by default

- **Choice:** if `ChannelService__BaseUrl` is set, Messaging asks Channel Management whether the channel exists. If unset, it accepts any channel id.
- **Why:** it demonstrates a synchronous dependency and a WireMock service test, but the service must still run on its own (the assignment says each service "should be able to run independently").
- **Trade-off:** when enabled, Messaging is unavailable for posting if Channel Management is down (temporal coupling). It answers `503` in that case.

## D8. Versions

- **Choice:** .NET 8, EasyNetQ 7.8.0 (same as the lecture repository), xUnit, Moq, WireMock.Net, FluentAssertions 7 (the last version with a free licence).
- **Why:** the teacher's Docker example is .NET 8. The task text shows .NET 7 images, which are end of life.

## Known limitations

| Limitation | Effect | Possible fix |
|---|---|---|
| Save then publish are two separate steps (dual write) | If the broker is down after the save, the event is lost | Transactional outbox |
| No authentication | Anyone can post as any author | Gateway with JWT, `AuthorId` taken from the token |
| No retry or dead-letter handling in the client | A failing handler loses the message after EasyNetQ's default retry | Dead-letter queues, explicit retry policy |
| No message history endpoint | Cannot list a channel's messages | `GET /api/channels/{id}/messages` with paging |
| In-memory storage | Data lost on restart | Real database |

Next: [07-work-plan.md](07-work-plan.md).
