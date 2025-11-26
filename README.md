
# Nexus

## Introduction
The **mcs** project (code: Nexus) is designed to manage real-time operations in automated environments such as semiconductor or warehouse systems.  
It leverages **Redis** for in-memory event-driven communication and **Blazor Server** for UI.  
The system follows an **event-driven architecture (EDA)** and is containerized using **Docker Compose** for easy deployment. [(https://portal.stone2on.cloud)](https://portal.stone2on.cloud)


Current active modules include:
- **Nexus.Core** – Domain models and services (areas, locations, lots, transports, stockers, etc.) with Redis-backed persistence.  
- **Nexus.Orchestrator** – Orchestration and coordination logic for lot/plan management, event handling, and system workflows.  
- **Nexus.Infrastructure** – Messaging (Redis Pub/Sub) and persistence implementations.  
- **Nexus.UI** – Blazor Server application for real-time monitoring and control.  

Removed modules:  
- `Nexus.Integrator` and `Nexus.Scheduler` were deprecated and replaced by consolidated orchestration services:contentReference[oaicite:1]{index=1}.  

---

## Architecture

```mermaid
graph TD
    subgraph Clients
        UI[Blazor UI<br/>Monitoring & Control]
    end

    subgraph Infrastructure
        Redis[(Redis Cache<br/>Pub/Sub, State)]
    end

    subgraph Services
        Orchestrator[Nexus.Orchestrator<br/>Lot & Plan Orchestration]
        Core[Nexus.Core<br/>Domain Models & State Management]
        Infra[Nexus.Infrastructure<br/>Persistence & Messaging]
    end

    UI --> Orchestrator
    Orchestrator --> Core
    Core --> Infra
    Infra --> Redis
    
````

---

## Features

* **Event-driven messaging** with Redis Pub/Sub.
* **Domain-driven design (DDD)** for areas, locations, lots, transports, and stockers.
* **Orchestration services** to coordinate lots, plans, and workflows.
* **Real-time Blazor UI** for monitoring and user interaction.
* **Docker Compose setup** for Redis, PostgreSQL, and Nexus services.

---


## Getting Started

### Prerequisites

* [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
* [Docker & Docker Compose](https://docs.docker.com/get-docker/)

### Run with Docker Compose

```bash
cd src
docker-compose up --build
```

This will start:

* Redis (port 6379)
* Nexus Orchestrator
* Nexus UI (Blazor Server)

---

## License

This project is released under the [Unlicense](LICENSE), making it free for public and private use.

