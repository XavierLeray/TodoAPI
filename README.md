\# TodoAPI



A RESTful API built with .NET 8 demonstrating Clean Architecture, CQRS, and modern development practices.



\## Architecture

```

TodoAPI.Domain          → Entities, Ports (interfaces), Business rules

TodoAPI.Application     → CQRS (Commands/Queries), DTOs, Validators, Service interfaces

TodoAPI.Infrastructure  → EF Core, Redis, JWT, Repository implementations

TodoAPI.API             → Controllers, Middlewares, Program.cs

```



\### Key Patterns

\- \*\*Clean Architecture\*\* with strict dependency rules (Domain → Application → Infrastructure → API)

\- \*\*CQRS\*\* with MediatR for command/query separation

\- \*\*Repository pattern\*\* with ports (interfaces) in Domain, implementations in Infrastructure



\## Tech Stack



| Component       | Technology                          |

|----------------|-------------------------------------|

| Framework      | .NET 8 (LTS)                        |

| ORM            | Entity Framework Core 8 (SQLite)    |

| CQRS           | MediatR 12                          |

| Validation     | FluentValidation 11                 |

| Authentication | JWT Bearer                          |

| Caching        | Redis via IDistributedCache         |

| Testing        | xUnit, Moq, FluentAssertions        |

| Containers     | Docker + docker-compose             |



\## Getting Started



\### Prerequisites

\- .NET 8 SDK

\- Docker (optional, for Redis)



\### Run locally

```bash

dotnet restore

dotnet build

dotnet run --project TodoAPI.API

```



\### Run with Docker

```bash

docker-compose up --build

```



\### Run tests

```bash

dotnet test

```



\## API Endpoints



| Method | Endpoint           | Auth     | Description        |

|--------|-------------------|----------|--------------------|

| GET    | /api/todos        | -        | List all todos     |

| GET    | /api/todos/{id}   | -        | Get todo by ID     |

| POST   | /api/todos        | Required | Create a todo      |

| PUT    | /api/todos/{id}   | Required | Update a todo      |

| DELETE | /api/todos/{id}   | Admin    | Delete a todo      |

| POST   | /api/auth/register| -        | Register a user    |

| POST   | /api/auth/login   | -        | Login (get JWT)    |



\## Observability



Three custom middlewares provide request tracing:

\- \*\*ExceptionMiddleware\*\* — Global error handling with ProblemDetails (RFC 7807)

\- \*\*CorrelationIdMiddleware\*\* — Unique request tracing via X-Correlation-Id header

\- \*\*RequestLoggingMiddleware\*\* — Request duration logging with slow request warnings (>500ms)

