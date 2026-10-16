# Shared.Contracts

Event types shared between Bizcord services. This package contains **contracts only**: plain data classes, no logic and no dependencies.

| Event | Published by | Consumed by |
|---|---|---|
| `MessagePostedEvent` | Messaging | Notifications, Presence, Search, Moderation, Analytics, Media, Realtime Gateway |

Rules:
- Every result event published by a consumer must include the `MessageId` it relates to, so a message can be traced through the system.
- The compiler guarantees the **shape** of an event. Contract tests guarantee the **values** (not `Guid.Empty`, not blank). The Messaging Service has a provider contract test, and each consumer should have a consumer contract test.
- Remove or rename a field only together with all consumers. Adding a field is safe.
