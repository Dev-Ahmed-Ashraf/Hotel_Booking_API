<div align="center">

# 🏨 Hotel Booking System

A modern, scalable hotel booking system built with **ASP.NET Core 8**, **Clean Architecture**, **CQRS**, and **Stripe Payments**.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-239120?logo=c-sharp&logoColor=white)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Build Status](https://github.com/Dev-Ahmed-Ashraf/Hotel_Booking_API/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Dev-Ahmed-Ashraf/Hotel_Booking_API/actions)
[![Docker Pulls](https://img.shields.io/docker/pulls/Dev-Ahmed-Ashraf/hotel-booking-api)](https://hub.docker.com/r/Dev-Ahmed-Ashraf/hotel-booking-api)

<img src="screenshots/swagger-ui-1.png" width="85%" alt="Swagger UI"/>

</div>

## ✨ Key Features

### For Hotel Guests

- Browse and search hotels with filters
- View room availability and pricing
- Secure online booking with instant confirmation
- Manage bookings and view history
- Leave reviews and ratings

### For Hotel Managers

- Manage hotel and room listings
- Handle bookings and check-ins/check-outs
- View occupancy reports and analytics
- Respond to guest reviews

### For Administrators

- Full system management
- User and role administration
- System configuration
- Advanced reporting

## 🛠️ Technology Stack

### 🏗 Architecture & Patterns

- **Clean Architecture** (Domain → Application → Infrastructure → API)
- **CQRS Pattern** using MediatR
- **Repository Pattern** + Unit of Work
- **Domain-Driven Design principles** (Entities, Value Objects, Domain Events)
- **Specification Pattern** for reusable queries
- **SOLID & Clean Code** principles

### ⚙ Backend Technologies

- **.NET 8 / ASP.NET Core 8**
- **Entity Framework Core 8** (SQL Server)
- **MediatR** (Commands / Queries / Pipeline Behaviors)
- **FluentValidation** for request validation
- **AutoMapper** for DTO → Entity mapping
- Built-in **Dependency Injection**

### 🔐 Authentication & Authorization

- **JWT Bearer Authentication**
- **Role-Based Access Control (RBAC)**
- Policy-based authorization
- Secure password hashing

### 🌐 API Capabilities

- RESTful API design
- **API Versioning**
- **Swagger / OpenAPI 3**
- Consistent API Response Wrapper
- Global Exception Handling
- **CORS** configuration
- **Response Caching**
- Response Compression

### 💾 Data & Persistence

- SQL Server 2022
- EF Core Migrations
- Soft Delete support
- Pagination, filtering, sorting
- Optimized EF Core queries (AsNoTracking, compiled queries)

### 💳 Payments

- **Stripe PaymentIntent API**
- Webhook Handling (`payment_intent.succeeded`)
- Automatic booking confirmation on payment success

### 🧪 Testing

- **xUnit** for unit testing
- **Integration Tests** using WebApplicationFactory/TestServer
- **Moq** for mocking
- Coverage reporting (Coverlet)

### 📊 Logging & Monitoring

- **Serilog** (Console + File + Seq support)
- Request/Response logging
- Structured logging with enrichers
- **Health Checks** (`/health`, `/health/ready`, `/health/live`)

### 🐳 DevOps & Deployment

- **Docker** + Docker Compose
- Environment-based configuration (Development/Production)
- GitHub Actions (CI pipeline)
- Secrets via Environment Variables / User Secrets

### 🔒 Security

- HTTPS enforcement
- Security headers (CSP, HSTS, XSS-Protection)
- Input validation
- Output encoding
- Rate limiting
- CORS Policies

### 🧰 Developer Experience

- Visual Studio 2022 / VS Code
- Postman Collection included
- Swagger UI for interactive testing
- XML documentation comments

## 🚀 Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (recommended)
- [Postman](https://www.postman.com/) (optional, for API testing)

### Run with Docker (Recommended)

```bash

#Clone the repository
git clone https://github.com/Dev-Ahmed-Ashraf/Hotel_Booking_API.git
cd Hotel_Booking_API

#Start the full stack (API + SQL Server)
docker-compose up -d --build
```

## 📸 Screenshots

<div align="center">

  <!-- Swagger UI -->
  <h3>Swagger Documentation</h3>
  <img src="screenshots/swagger-ui-1.png" width="80%" />
  <img src="screenshots/swagger-ui-2.png" width="80%" />
  <img src="screenshots/swagger-ui-3.png" width="80%" />
  <img src="screenshots/swagger-ui-4.png" width="80%" />

  <!-- Postman -->
  <h3>Postman Collection</h3>
  <img src="screenshots/postman-collection.png" width="80%" />

  <!-- JWT Authentication -->
  <h3>JWT Authentication</h3>
  <img src="screenshots/jwt-authentication.png" width="80%" />

  <!-- API Response Examples -->
  <h3>API Response Examples</h3>
  <img src="screenshots/api-response-example-gethotels.png" width="80%" />
  <img src="screenshots/api-response-example-CreateBooking.png" width="80%" />
  <img src="screenshots/api-response-example-DeleteHotel.png" width="80%" />
  <img src="screenshots/api-response-example-PartialUpdateRoom.png" width="80%" />
  <img src="screenshots/api-response-example-GetReviewsForHotel.png" width="80%" />

  <!-- Admin Dashboard -->
  <h3>Admin Dashboard</h3>
  <img src="screenshots/api-response-example-admindashboard.png" width="80%" />

  <!-- Database Schema -->
  <h3>Database Schema</h3>
  <img src="screenshots/database-schema.png" width="80%" />

</div>

## 📚 Documentation

For detailed technical documentation, please refer to:

- [API Documentation](Hotel_Booking_API/README.md) - Complete API reference and developer guide
- [Database Schema](/docs/DATABASE.md) - Detailed database design and relationships
- [Authentication Guide](/docs/AUTHENTICATION.md) - Setting up authentication and authorization
- [Deployment Guide](docs/DEPLOYMENT.md) - Production deployment instructions

## 🛠 Built With

- ASP.NET Core 8
- EF Core 8
- MediatR
- FluentValidation
- AutoMapper
- Serilog
- xUnit
- Docker

## 🤝 Contributing

Contributions are what make the open-source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [.NET Foundation](https://dotnetfoundation.org/)
- [MediatR](https://github.com/jbogard/MediatR)
- [Serilog](https://serilog.net/)
- [Stripe](https://stripe.com/)
- [Swagger](https://swagger.io/)
