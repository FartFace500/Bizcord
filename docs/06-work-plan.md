# 6. Work plan: weeks, branches and deliverables

Use this page to navigate the repository. Each course week has its own branch, merged into `main` with a merge commit so the history shows the work per week.

## Swimlane: what is delivered, by whom, when

Each lane is an area of the project. Each box is a deliverable. Arrows show what builds on what.

```mermaid
flowchart LR
    subgraph L1["Lane: Documentation"]
        direction LR
        D1["W36: Domains, boundaries,<br/>interactions (docs/)"]
        D2["README + diagrams updated<br/>as code lands"]
    end

    subgraph L2["Lane: Repository and structure"]
        direction LR
        R1["W37 Lab 1: branch chore/initialize-project,<br/>src/ tests/ Dockerfile README"]
        R2["W37 Lab 2: .NET web API in<br/>apps/messaging-microservice"]
    end

    subgraph L3["Lane: Shared packages"]
        direction LR
        P1["W37 Lab 3: MessageClient<br/>(IMessageClient + EasyNetQ adapter)"]
        P2["W40: Shared.Contracts<br/>MessagePostedEvent"]
    end

    subgraph L4["Lane: Deployment"]
        direction LR
        X1["W39 Task 1: multi-stage<br/>Dockerfile"]
        X2["W39 Task 2: docker-compose<br/>RabbitMQ + service + healthcheck"]
    end

    subgraph L5["Lane: Feature and tests"]
        direction LR
        F1["W40: Message domain,<br/>POST/GET /api/messages"]
        F2["W40: unit, provider contract,<br/>service (WireMock), integration tests"]
    end

    D1 --> R1
    R1 --> R2
    R2 --> P1
    P1 --> X1
    X1 --> X2
    P2 --> F1
    X2 --> F1
    F1 --> F2
    F2 --> D2
```

## Branch and merge order

```mermaid
flowchart TB
    M0(["main"]) --> B1["docs/analysis-and-design<br/>W36: design docs"]
    B1 -- merge --> M1(["main"])
    M1 --> B2["chore/initialize-project<br/>W37 lab 1 and 2: structure and boilerplate"]
    B2 -- merge --> M2(["main"])
    M2 --> B3["feat/message-client<br/>W37 lab 3: MessageClient"]
    B3 -- merge --> M3(["main"])
    M3 --> B4["chore/dockerize<br/>W39: Dockerfile and Compose"]
    B4 -- merge --> M4(["main"])
    M4 --> B5["feat/post-message-and-tests<br/>W40: feature, contracts and tests"]
    B5 -- merge --> M5(["main"])
```

Each branch starts from the latest `main` and is merged back before the next one starts. Rounded boxes are `main` after each merge.

## Where to find each task

| Task | Week | Branch | What to look at |
|---|---|---|---|
| 1. Analysis and design | 36 | `docs/analysis-and-design` | `docs/01` to `docs/06` |
| 2a. Setup and boilerplate | 37 | `chore/initialize-project` | `apps/messaging-microservice/` (`src/`, `tests/`, `Dockerfile`, `README.md`) |
| 2b. Message client | 37 | `feat/message-client` | `packages/MessageClient/` |
| 4. Deployment | 39 | `chore/dockerize` | `apps/messaging-microservice/Dockerfile`, `docker-compose.yml`, `.dockerignore` |
| 5. First feature and testing | 40 | `feat/post-message-and-tests` | `packages/Shared.Contracts/`, `apps/messaging-microservice/src/` and `tests/` |

## Files that several weeks touch

Because the weeks build on each other, these files change more than once. The merge order above avoids conflicts, since each branch starts from the latest `main`.

| File | Changed in |
|---|---|
| `apps/messaging-microservice/src/Program.cs` | W37 (boilerplate), W37 (register message client), W40 (register repository and handlers) |
| `apps/messaging-microservice/src/Messaging.csproj` | W37, W37 (package refs), W40 |
| `apps/messaging-microservice/README.md` | W37, W37 (message client), W39, W40 |
| `apps/messaging-microservice/Dockerfile` | W39, W40 (copy `Shared.Contracts`) |
| `Bizcord.sln` | W37 and W40 |
| `docs/05-messaging-service-design.md` | W36 (plan), W40 (reconciled with real code) |
