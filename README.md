# Hotel Booking API (ASP.NET Core 8)

A production-grade, Clean Architecture hotel booking API with CQRS (MediatR), EF Core, JWT auth, caching, and Stripe payments. Includes Docker, CI, and professional documentation.

## Highlights
- Clean Architecture (Domain, Application, Infrastructure, API)
- CQRS with MediatR, FluentValidation, AutoMapper
- EF Core (SQL Server) with Migrations
- JWT Authentication and role-based Authorization
- Serilog structured logging + request logging
- Caching (memory), rate limiting, response compression, output caching
- Global exception handling and consistent API envelope
- Stripe integration (test mode)
- Docker & GitHub Actions CI

## Tech Stack
- .NET 8, ASP.NET Core 8, EF Core 8
- MediatR, FluentValidation, AutoMapper
- Serilog, Swagger / OpenAPI
- Stripe .NET SDK

## Repository Structure
```
Hotel_Booking_API_Project/
├── Hotel_Booking_API/           # API project (main)
├── tests/
│   ├── Hotel_Booking_API.UnitTests/
│   └── Hotel_Booking_API.IntegrationTests/
├── .github/workflows/dotnet.yml # CI pipeline
├── Dockerfile                   # API container
├── docker-compose.yml           # API + SQL Server (dev)
├── README.md                    # This file
└── appsettings.example.json     # Example configuration
```

## Quickstart (Local)
1. Prerequisites: .NET 8 SDK, SQL Server (or Docker), PowerShell
2. Clone and restore:
   ```bash
   git clone <your-repo-url>
   cd Hotel_Booking_API_Project/Hotel_Booking_API
   dotnet restore
   ```
3. Configure development settings:
   - Copy `appsettings.example.json` to `appsettings.Development.json` and fill secrets using User Secrets or env vars.
4. Apply migrations and run:
   ```bash
   dotnet ef database update
   dotnet run
   ```
5. Open Swagger UI at `https://localhost:7062/swagger` (port may vary).

## Quickstart (Docker)
1. Create a `.env` with your secrets (see Variables below).
2. Start the stack:
   ```bash
   docker compose up -d --build
   ```
3. API: `https://localhost:8080` (proxied)

### docker-compose services
- `sqlserver`: SQL Server 2022 Developer, persisted volume
- `api`: ASP.NET Core app, depends_on database

## Configuration
Sensitive values should come from env vars or User Secrets. See `appsettings.example.json`.

Key variables:
- ConnectionStrings__DefaultConnection
- JwtSettings__SecretKey, JwtSettings__Issuer, JwtSettings__Audience, JwtSettings__ExpirationMinutes
- Stripe__ApiKey, Stripe__WebhookSecret, Stripe__PublishableKey, Stripe__Currency
- SmtpSettings__Host, SmtpSettings__Port, SmtpSettings__Username, SmtpSettings__Password, SmtpSettings__SenderEmail, SmtpSettings__SenderName, SmtpSettings__EnableSsl
- Cors__AllowedOrigins (array in JSON or `;`-separated)

## Development Notes
- Use migrations; do not rely on EnsureCreated in production.
- Keep logs out of the repo; configure `.gitignore` for `logs/`.
- Re-enable `[Authorize]` on protected endpoints; use role policies.

## Testing
Run all tests:
```bash
dotnet test tests --configuration Release
```

## CI
GitHub Actions builds, tests, and publishes artifacts for pushes/PRs to `main`/`master`.

## API Documentation
- Swagger UI: `/swagger`
- Postman collections are provided in `Hotel_Booking_API/`.

## License
MIT

---
Built with ASP.NET Core 8 and Clean Architecture.

