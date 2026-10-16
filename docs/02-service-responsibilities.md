# 2. Service responsibilities

For each service: what it **does**, what it explicitly **does not do**, and how others reach it. Only the Messaging Service is implemented in this repository. The rest are design-only. Names marked `(implemented)` exist in code, the rest are proposed.

## Summary table

| Service | Owns | Publishes | Subscribes to | Status |
|---|---|---|---|---|
| **Messaging** | Messages | `MessagePostedEvent` | (future) `MessageFlaggedEvent`, `ChannelDeletedEvent` | **Implemented** |
| Channel Management | Workspaces, channels, membership, roles | `ChannelCreatedEvent`, `ChannelDeletedEvent` | `UserDeletedEvent` | Design only |
| User Profiles | Users, display names, avatars | `UserDeletedEvent` | none | Design only |
| Realtime Gateway | WebSocket connections (in memory only) | none | `MessagePostedEvent`, `MentionDetectedEvent`, `ReactionAddedEvent` | Design only |
| Presence | Online status, last active time | `UserStatusChangedEvent` | `MessagePostedEvent` | Design only |
| Notifications | Notification records, delivery preferences | `MentionDetectedEvent` | `MessagePostedEvent`, `ReactionAddedEvent` | Design only |
| Reactions | Reactions on messages | `ReactionAddedEvent` | `MessageDeletedEvent` | Design only |
| Search | Full-text index | `MessageIndexedEvent` | `MessagePostedEvent` | Design only |
| Moderation | Banned-word rules, flags | `MessageFlaggedEvent` | `MessagePostedEvent` | Design only |
| Media | Attachment files and metadata | `AttachmentProcessedEvent` | `MessagePostedEvent` | Design only |
| Analytics | Counters, reports | none | `MessagePostedEvent`, `ReactionAddedEvent` | Design only |

## Messaging Service (implemented)

**Responsibilities**
1. Accept a new message from a client and **validate** it (author, channel and content are present, content is not blank, content is not too long).
2. **Persist** the message.
3. **Announce** that the message was posted by publishing `MessagePostedEvent`.
4. Return a stored message by id (and, later, the history of a channel).
5. (Optional, week 40 extra) Ask the Channel service whether the channel exists before accepting the message.

**Not responsible for**
- Who is allowed to post in a channel (Channel Management).
- Whether the author exists or what their name is (User Profiles).
- Detecting mentions, indexing, moderation, attachments or counting (the consumers of the event).
- Pushing the message to connected clients (Realtime Gateway).
- Reactions, which belong to a separate domain that refers to a message id.

**Interfaces**

| Direction | Interface | Style | Purpose |
|---|---|---|---|
| In | `POST /api/messages` | REST | Post a message |
| In | `GET /api/messages/{id}` | REST | Read one message |
| Out | `MessagePostedEvent` | Event on RabbitMQ | Tell everyone a message was posted |
| Out (optional) | `GET /api/channels/{id}` on Channel service | REST | Check the channel exists |

## Why events, and why one result event per service

- The Messaging Service publishes **one fact** (`MessagePostedEvent`) and never knows who listens. A new consumer needs no change in Messaging (low coupling).
- Each consumer publishes its **own result event** and includes `MessageId`, so a message can be traced end to end through the system. For example: `MessagePostedEvent(MessageId=X)` then `MentionDetectedEvent(MessageId=X)`.
- The Messaging Service itself must stay fast. It does not wait for moderation or indexing before replying to the user, so those run asynchronously after the message is saved.

## Other services in one paragraph each

- **Channel Management**: the source of truth for channels and who belongs to them. Provides a lookup API (`GET /api/channels/{id}`) used for validation and publishes channel lifecycle events so other services can clean up.
- **User Profiles**: the source of truth for people. Other services keep only a `UserId`.
- **Realtime Gateway**: keeps WebSocket connections open and forwards events (new message, mention, reaction) to the right clients. It stores nothing permanently.
- **Presence**: keeps a "last active" time per user, updated from message events and heartbeats. Data is short-lived.
- **Notifications**: parses `Content` for `@mentions` and publishes `MentionDetectedEvent`. Decides push, email or in-app, based on user preferences.
- **Reactions**: stores which user reacted with which emoji to which message id.
- **Search**: builds a searchable index from message content.
- **Moderation**: scans content against rules and publishes `MessageFlaggedEvent` when needed. Messaging can later hide flagged messages.
- **Media**: handles attachments (upload, virus scan, thumbnails). A message only holds attachment ids.
- **Analytics**: counts messages per channel and builds reports. It never writes back to other services.

Next: [03-boundaries-and-data-ownership.md](03-boundaries-and-data-ownership.md).
